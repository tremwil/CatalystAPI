using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Catalyst.Settings
{
    /// <summary>
    /// A class to handle simple JSONPath lists pointing to values.
    /// </summary>
    public static class JsonPathList
    {
        /// <summary>
        /// Create a JSON object from the path list.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="baseObj">A default object to use.</param>
        /// <returns></returns>
        public static JObject FromFile(string filePath, JObject baseObj = null)
        {
            string[] vsplit;
            string[] pnames;

            JObject json = baseObj ?? new JObject();
            JToken lastRoot;
            object prop;
            string value;

            foreach (string vpath in File.ReadLines(filePath))
            {
                lastRoot = json;

                vsplit = vpath.Split(' ');
                pnames = vsplit[0].Split('.');

                value = vsplit[1];                   

                for (int i = 0; i < pnames.Length; i++)
                {
                    prop = pnames[i];
                    if (char.IsDigit(pnames[i][0]))
                        prop = Convert.ToInt32(prop);

                    if (i == pnames.Length - 1)
                    {
                        SafeSet(
                            lastRoot, 
                            prop, 
                            value.Contains(".")?
                                new JValue(Convert.ToSingle(value)) : 
                                new JValue(Convert.ToInt32(value))
                        );
                        break;
                    }

                    else if (SafeGet(lastRoot, prop) == null)
                    {
                        if (char.IsDigit(pnames[i + 1][0]))
                            SafeSet(lastRoot, prop, new JArray());
                        else
                            SafeSet(lastRoot, prop, new JObject());
                    }

                    lastRoot = lastRoot[prop];
                }
            }

            return json;
        }

        private static JToken SafeGet(JToken token, object prop)
        {
            if (token.Type == JTokenType.Array)
            {
                if ((int)prop >= token.Count()) return null;
                else return token[prop];
            }

            return token[prop];
        }

        private static void SafeSet(JToken token, object prop, JToken value)
        {
            if (token.Type == JTokenType.Array)
            {
                var arr = (JArray)token;
                while ((int)prop >= token.Count()) arr.Add(null);

                token[prop] = value;
            }

            else token[prop] = value;
        }

        /// <summary>
        /// Create a JSONPath list file from a JSON object
        /// </summary>
        /// <param name="data">The JSON object to convert.</param>
        /// <param name="path">The path of the file.</param>
        public static void ToFile(JObject data, string path)
        {
            var paths = new List<string>();

            // Build the list
            BuildJSONPathList(data, paths);

            // Sort in alphabetical order
            paths.Sort();

            // Write the paths to the file
            File.WriteAllLines(path, paths.ToArray());
        }

        private static void BuildJSONPathList(JToken current, List<string> paths)
        {
            string path;
            string value;
            JToken mchild;

            foreach (var child in current)
            {
                // JObject iterates over propeties but JArray does it directly over values
                mchild = child;
                if (mchild.Type == JTokenType.Property)
                    mchild = ((JProperty)mchild).Value;

                if (mchild.Type == JTokenType.Array || mchild.Type == JTokenType.Object)
                    BuildJSONPathList(mchild, paths);

                else
                {
                    // Make indices look like normal properties
                    // ex: stuff[0].type -> stuff.0.type
                    path = mchild.Path
                        .Replace('[', '.')
                        .Replace("]", "");

                    // Fix floats to 6 decimal places
                    if (mchild.Type == JTokenType.Float)
                        value = ((float)mchild).ToString("f6");

                    else value = mchild.ToString();

                    paths.Add(path + " " + value);
                }
            }
        }
    }
}
