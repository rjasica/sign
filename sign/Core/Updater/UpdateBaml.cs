using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using Mono.Cecil;

namespace Sign.Core.Updater
{
    public class UpdateBaml : IUpdater
    {
        private readonly Regex regex;

        public UpdateBaml()
        {
            const string Size = @"[\u0080-\u00FF]{0,4}[\u0000-\u0079]";
            this.regex =
                new Regex(
                    @"(?<marker>\u001C)(?<totalsize>" + Size + 
                    @")(?<id>..)(?<size>" + Size +
                    @")(?<name>(?:\w+\.)*\w+), Version=(?<version>(?:\d+\.){3}\d+), Culture=(?<culture>(?:\w|\-)+), PublicKeyToken=(?<token>null|(?:\d|[abcdef]){16})",
                    RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public void Update(StrongNameKeyPair snk, HashSet<IAssemblyInfo> notSigned, IEnumerable<IAssemblyInfo> allAssemblies)
        {
            var token = GetKeyTokenFromFullKey(snk);

            foreach (var assemblyInfo in notSigned.ToArray())
            {
                foreach (var module in assemblyInfo.Assembly.Modules)
                {
                    this.UpdareResource(notSigned, token, assemblyInfo, module);
                }
            }
        }

        private static void ReplaceResource(
            ModuleDefinition module,
            int resIndex,
            Resource resource,
            MemoryStream memoryStream,
            ResourceWriter rw )
        {
            module.Resources.RemoveAt(resIndex);
            rw.Generate();
            var array = memoryStream.ToArray();
            memoryStream.Position = 0;
            var newEmbeded = new EmbeddedResource(resource.Name, resource.Attributes, array);

            module.Resources.Insert(resIndex, newEmbeded);
        }

        private static void UptadateBinaryBaml(
            string token,
            ResourceWriter rw,
            string resourceName,
            List<char> charList,
            List<Match> elementsToReplace )
        {
            elementsToReplace = elementsToReplace.OrderBy(x => x.Index).ToList();

            var buffer = new MemoryStream();

            using (var bufferWriter = new BinaryWriter(buffer))
            {
                for (var i = 0; i < charList.Count; i++)
                {
                    if (elementsToReplace.Count > 0 && elementsToReplace[0].Index == i)
                    {
                        var match = elementsToReplace[0];
                        bufferWriter.Write((byte)0x1C);

                        var newAssembly =
                            string.Format(
                                "{0}, Version={1}, Culture={2}, PublicKeyToken={3}",
                                match.Groups["name"].Value,
                                match.Groups["version"].Value,
                                match.Groups["culture"].Value,
                                token);

                        var length = Get7BitEncoded(newAssembly.Length).Length
                                     + newAssembly.Length + 3;
                        var totalLength = Get7BitEncoded(length);
                        bufferWriter.Write(totalLength);

                        var id = match.Groups["id"].Value;
                        bufferWriter.Write((byte)id[0]);
                        bufferWriter.Write((byte)id[1]);
                        bufferWriter.Write(newAssembly);

                        i += match.Length - 1;

                        elementsToReplace.RemoveAt(0);
                    }
                    else
                    {
                        var b = (byte)charList[i];
                        bufferWriter.Write(b);
                    }
                }

                bufferWriter.Flush();

                var mst = new MemoryStream(buffer.ToArray());
                rw.AddResource(resourceName, mst);
            }
        }

        private static string GetKeyTokenFromFullKey(StrongNameKeyPair snk)
        {
            var csp = new SHA1CryptoServiceProvider();
            var hash = csp.ComputeHash(snk.PublicKey);
            var token = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                token[i] = hash[hash.Length - (i + 1)];
            }

            return string.Join( string.Empty, token.Select(x => x.ToString("x2")));
        }

        private static byte[] Get7BitEncoded(int value)
        {
            var list = new List<byte>();
            var num = (uint)value;

            while (num >= 128U)
            {
                list.Add((byte)(num | 128U));
                num >>= 7;
            }

            list.Add((byte)num);
            return list.ToArray();
        }

        private void UpdareResource(HashSet<IAssemblyInfo> modified, string token, IAssemblyInfo assemblyInfo, ModuleDefinition module)
        {
            var resArray = module.Resources.ToArray();
            for (var resIndex = 0; resIndex < resArray.Length; resIndex++)
            {
                var resource = resArray[resIndex];
                if (resource.ResourceType == ResourceType.Embedded)
                {
                    if(!resource.Name.EndsWith(".g.resources"))
                    {
                        continue;
                    }

                    var embededResource = (EmbeddedResource)resource;
                    var modResource = false;

                    var memoryStream = new MemoryStream();
                    var rw = new ResourceWriter(memoryStream);

                    var stream = embededResource.GetResourceStream();
                    var reader = new ResourceReader(stream);
                    foreach (var entry in reader.OfType<DictionaryEntry>().ToArray())
                    {
                        var resourceName = entry.Key.ToString();
                        var resourceStream = entry.Value as Stream;

                        if (resourceStream != null && resourceName.EndsWith(".baml", StringComparison.InvariantCulture))
                        {
                            modResource = this.CheckBaml(modified, token, assemblyInfo, modResource, rw, resourceName, resourceStream);
                        }
                        else
                        {
                            rw.AddResource(resourceName, entry.Value);
                        }
                    }

                    if (modResource)
                    {
                        ReplaceResource(module, resIndex, resource, memoryStream, rw);
                    }
                }
            }
        }

        private bool CheckBaml(
            HashSet<IAssemblyInfo> modified,
            string token,
            IAssemblyInfo assemblyInfo,
            bool modResource,
            ResourceWriter rw,
            string resourceName,
            Stream resourceStream )
        {
            var br = new BinaryReader(resourceStream);
            var datab = br.ReadBytes((int)br.BaseStream.Length);

            var charList = datab.Select( b => (char)b ).ToList();

            var data = new string( charList.ToArray() );

            var elementsToReplace = new List<Match>();

            var matches = this.regex.Matches(data);
            foreach (Match match in matches)
            {
                var name = match.Groups["name"].Value;
                if (modified.Any(x => x.Assembly.Name.Name == name))
                {
                    elementsToReplace.Add(match);
                }
            }

            if (elementsToReplace.Count != 0)
            {
                modified.Add(assemblyInfo);
                modResource = true;

                UptadateBinaryBaml( token, rw, resourceName, charList, elementsToReplace);
            }
            else
            {
                resourceStream.Position = 0;
                rw.AddResource(resourceName, resourceStream);
            }

            return modResource;
        }
    }
}
