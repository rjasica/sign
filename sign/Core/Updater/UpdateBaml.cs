using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Sign.Core.Updater
{
    public class UpdateBaml : IUpdater
    {
        private Regex regex;
        public UpdateBaml()
        {
            var size = @"[\u0080-\u00FF]{0,4}[\u0000-\u0079]";
            this.regex =
                new Regex(
                    @"(?<marker>\u001C)(?<totalsize>" + size + 
                    @")(?<id>..)(?<size>" + size +
                    @")(?<name>(?:\w+\.)*\w+), Version=(?<version>(?:\d+\.){3}\d+), Culture=(?<culture>(?:\w|\-)+), PublicKeyToken=(?<token>null|(?:\d|[abcdef]){16})",
                    RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public void Update(System.Reflection.StrongNameKeyPair snk, HashSet<IAssemblyInfo> modified, IEnumerable<IAssemblyInfo> allAssemblies)
        {
            var token = GetKeyTokenFromFullKey(snk);

            foreach (var assemblyInfo in modified.ToArray())
            {
                foreach (var module in assemblyInfo.Assembly.Modules)
                {
                    UpdareResource(modified, token, assemblyInfo, module);
                }
            }
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

                    EmbeddedResource embededResource = (EmbeddedResource)resource;
                    bool modResource = false;

                    MemoryStream memoryStream = new MemoryStream();
                    ResourceWriter rw = new ResourceWriter(memoryStream);

                    Stream stream = embededResource.GetResourceStream();
                    ResourceReader reader = new ResourceReader(stream);
                    foreach (DictionaryEntry entry in reader.OfType<DictionaryEntry>().ToArray())
                    {
                        string resourceName = entry.Key.ToString();
                        Stream resourceStream = entry.Value as Stream;

                        if (resourceStream != null && resourceName.EndsWith(".baml", StringComparison.InvariantCulture))
                        {
                            modResource = CheckBaml(modified, token, assemblyInfo, modResource, rw, resourceName, resourceStream);
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

        private bool CheckBaml(HashSet<IAssemblyInfo> modified, string token, IAssemblyInfo assemblyInfo, bool modResource, ResourceWriter rw, string resourceName, Stream resourceStream)
        {
            BinaryReader br = new BinaryReader(resourceStream);
            byte[] datab = br.ReadBytes((int)br.BaseStream.Length);

            List<char> cList = new List<char>();
            foreach (byte b in datab) cList.Add((char)b);
            string data = new string(cList.ToArray());

            List<Match> toReplace = new List<Match>();

            var matches = regex.Matches(data);
            foreach (Match match in matches)
            {
                string name = match.Groups["name"].Value;
                if (modified.Any(x => x.Assembly.Name.Name == name))
                {
                    toReplace.Add(match);
                }
            }

            if (toReplace.Count != 0)
            {
                modified.Add(assemblyInfo);
                modResource = true;

                UptadateBinaryBaml(modified, token, ref modResource, rw, resourceName, cList, toReplace);
            }
            else
            {
                resourceStream.Position = 0;
                rw.AddResource(resourceName, resourceStream);
            }
            return modResource;
        }

        private static void ReplaceResource(ModuleDefinition module, int resIndex, Resource resource, MemoryStream memoryStream, ResourceWriter rw)
        {
            module.Resources.RemoveAt(resIndex);
            rw.Generate();
            var array = memoryStream.ToArray();
            memoryStream.Position = 0;
            var newEmbeded = new EmbeddedResource(resource.Name, resource.Attributes, array);

            module.Resources.Insert(resIndex, newEmbeded);
        }

        private static void UptadateBinaryBaml(HashSet<IAssemblyInfo> modified, string token, ref bool modResource, ResourceWriter rw, string resourceName, List<char> cList,  List<Match> toReplace)
        {
            toReplace = toReplace.OrderBy(x => x.Index).ToList();

            MemoryStream buffer = new MemoryStream();

            using (BinaryWriter bufferWriter = new BinaryWriter(buffer))
            {
                for (var i = 0; i < cList.Count; i++)
                {
                    if (toReplace.Count > 0 && toReplace[0].Index == i)
                    {
                        Match match = toReplace[0];
                        bufferWriter.Write((byte)0x1C);

                        string newAssembly =
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

                        toReplace.RemoveAt(0);
                    }
                    else
                    {
                        byte b = (byte)cList[i];
                        bufferWriter.Write(b);
                    }
                }

                bufferWriter.Flush();

                var array = buffer.ToArray();
                var newData = new string(array.Select(x => (char)x).ToArray());

                MemoryStream mst = new MemoryStream(buffer.ToArray());
                rw.AddResource(resourceName, mst);
            }
        }

        private static string GetKeyTokenFromFullKey(StrongNameKeyPair snk)
        {
            SHA1CryptoServiceProvider csp = new SHA1CryptoServiceProvider();
            byte[] hash = csp.ComputeHash(snk.PublicKey);
            byte[] token = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                token[i] = hash[hash.Length - (i + 1)];
            }

            return string.Join("", token.Select(x => x.ToString("x2")));
        }

        private static byte[] Get7BitEncoded(int value)
        {
            List<byte> list = new List<byte>();
            uint num = (uint)value;
            while (num >= 128U)
            {
                list.Add((byte)(num | 128U));
                num >>= 7;
            }
            list.Add((byte)num);
            return list.ToArray();
        }
    }
}
