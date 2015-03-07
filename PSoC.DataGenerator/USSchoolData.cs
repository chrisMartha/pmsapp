using System;
using System.Collections.Generic;

namespace PSoC.DataGenerator
{
    public partial class USSchoolData
    {
        
        public Guid PSocSchoolId { get; set; }

        public Guid PSocDistrictId { get; set; }

        
        public string NCES_School_ID { get; set; }

        
        public string State_School_ID { get; set; }

        
        public string NCES_District_ID { get; set; }

        
        public string State_District_ID { get; set; }

        
        public string Low_Grade { get; set; }

        
        public string High_Grade { get; set; }

        
        public string Grades { get; set; }

        
        public string School_Name { get; set; }

        
        public string District { get; set; }

        
        public string County_Name { get; set; }

        
        public string Street_Address { get; set; }

        
        public string City { get; set; }

        
        public string State { get; set; }

        
        public string ZIP { get; set; }

        
        public string ZIP_4digit { get; set; }

        
        public string Phone { get; set; }

        
        public string Locale_Code { get; set; }

        
        public string Locale { get; set; }

        
        public string Charter { get; set; }

        
        public string Magnet { get; set; }

        
        public string Title_I_School { get; set; }

        
        public string Title_1_School_Wide { get; set; }

        public decimal? Students { get; set; }

        public decimal? Teachers { get; set; }

        public decimal? Student_Teacher_Ratio { get; set; }

        public int? Free_Lunch { get; set; }

        public int? Reduced_Lunch { get; set; }
    }
}
