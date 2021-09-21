using System;
using System.Collections.Generic;

namespace DemoScanner.DemoStuff.GoldSource.Verify
{
    public class Config
    {
        public List<Tuple<string, Commandtype>> BaseRules;
        public string bxt_version = "";

        public List<Category> categories;

        public Config(string file)
        {
            BaseRules = new List<Tuple<string, Commandtype>>();
            categories = new List<Category>();
        }
    }
}