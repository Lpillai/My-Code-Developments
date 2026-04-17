using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeVariables
{

    public class item_net_weight
    {
        public int P21URL { get; set; }

        [DisplayName("Item ID")]
        public string item_id { get; set; }

        [DisplayName("Gross Weight")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public double? weight { get; set; }

        [DisplayName("LB to KG")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public double? toKG { get; set; }

        [DisplayName("Kilograms")]
        [DisplayFormat(DataFormatString = "{0:N4}", ConvertEmptyStringToNull = true, NullDisplayText = "0")]
        public double? kilograms { get; set; }
    }

}
