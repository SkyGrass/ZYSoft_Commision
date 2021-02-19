using System;
using static Commission.Api.Entities.Enums.CommonEnum;

namespace Commission.Api.ViewModels.Base.Comparison
{ 
    /// <summary>
    /// 
    /// </summary>
    public class ComparisonSelectJsonModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

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
    }
}
