using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.FG.FFL.Layouts.SL.FG.FFL.Model
{
    public class Pair
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class MSAContact
    {
        public int ContactId { get; set; }
        public string ContactDetail { get; set; }
    }
    public class MSARecommendation
    {
        public int RecommendationId { get; set; }
        public string RecommendationNo { get; set; }
        public string Description { get; set; }
        public string TypeOfVoilation { get; set; }
        public string RPUsername { get; set; }
        public string RPEmail { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string InjuryClass { get; set; }
        public string ObservationCategory { get; set; }
        public string ObservationSubcategory { get; set; }
        public bool ConsentTaken { get; set; }
        public string TargetDate { get; set; }
        public bool ObservationSpot { get; set; }
        public string Status { get; set; }
        public bool IsSavedAsDraft { get; set; }
        public string AssigneeUsername { get; set; }
        public string AssigneeEmail { get; set; }
        public int ValidationStatus { get; set; } //0: valid, 1:responsiblePerson not found; 2:Target Date not valid; 3: Target date must be greater than or equal to MSA date
        public SPUser ResponsiblePerson { get; set; }
    }
    public class MSA
    {
        public int MSAId { get; set; }
        public string MSADate { get; set; }
        public string AccompaniedBy { get; set; }
        public string AuditedBy { get; set; }
        public string Designation { get; set; }
        public string AreaAudited { get; set; }
        public int AreaAuditedId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string NoOfUnsafeActs { get; set; }
        public string NoOfUnsafeConditions { get; set; }
        public string NoOfSeriousInjury { get; set; }
        public string NoOfFatalityInjury { get; set; }
        public string PositivePoints { get; set; }
        public string AreaOfImprovement { get; set; }
        public string NoOfSafetyContacts { get; set; }
        public bool IsSavedAsDraft { get; set; }

    }

    public class CommonDictionary
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int SortOrder { get; set; }
    }
}
