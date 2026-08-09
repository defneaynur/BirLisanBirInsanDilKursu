using DilKursu.Entities.Common;

namespace DilKursu.Entities;

public class Branch : BaseEntity
{
 
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PublicTransportInstructions { get; set; } = string.Empty;

    public string CarTransportInstructions { get; set; } = string.Empty;

    public string SocialFacilities { get; set; } = string.Empty;

    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();

    public ICollection<Course> Courses { get; set; } = new List<Course>();

    public ICollection<TeacherBranch> TeacherBranches { get; set; } = new List<TeacherBranch>();
}
