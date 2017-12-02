using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DictEtLogic
{
    public class Dict
    {
        //UI wants an ObservableCollection
        public ObservableCollection<string> Words = new ObservableCollection<string>();
        public Dictionary<string, string> WordDescs = new Dictionary<string, string>();

        public string Lookup;  

        public Dict()
        {
            Lookup = "";
        }
    }
}
