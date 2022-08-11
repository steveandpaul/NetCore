using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NetCoreDBModule.NetCoreDB.DBDriver.RemoteDriver
{
    public class HashList : Hashtable
    {
        private ArrayList alKey = new ArrayList();
        private ArrayList alValue = new ArrayList();

        public override void Add(object key, object value)
        {
            alKey.Add(key);
            alValue.Add(value);
            base.Add(key, value);
        }
        public override void Clear()
        {
            alKey.Clear();
            alValue.Clear();
            base.Clear();
        }
        public override void Remove(object key)
        {
            alKey.Remove(key);
            alValue.Remove(base[key]);
            base.Remove(key);
        }
        public override ICollection Keys
        {
            get
            {
                return alKey;
            }
        }

        public override ICollection Values
        {
            get
            {
                return alValue;
            }
        }

        public void Sort()
        {
            alKey.Sort();
            alValue.Sort();
        }
    }
}
