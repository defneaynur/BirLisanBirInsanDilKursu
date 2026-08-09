namespace DilKursu.Business.Dtos;

public class CourseOccupancyDto
{
    public string CourseName { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public int Enrolled { get; set; }

    public int Quota { get; set; }

    public int OccupancyPercent { get; set; }
}

public class NameCountDto
{
    public string Name { get; set; } = string.Empty;

    public int Count { get; set; }
}
