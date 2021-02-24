using NPinyin;
using System;
using System.Text;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Base.Salesman
{
    /// <summary>
    /// 
    /// </summary>
    public class SalesmanSelectJsonModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        public string Code { get; set; }

        public int Value
        {
            get
            {
                return Id;
            }
        }

        public string text
        {
            get
            {
                return Name;
            }
        }
        public string sword
        {
            get
            {
                string[] pinyins = Pinyin.GetPinyin(Name).ToUpper().Split(" ");
                string str="";
                foreach (string pinyin in pinyins)
                {
                    str += pinyin[0];
                }
                return Code + "_" + str;
            }
        }
    }
}
