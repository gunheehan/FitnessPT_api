using System;
using System.Collections.Generic;

namespace FitnessPT_api.Models;

public partial class Exercisecategory
{
    public int CategoryId { get; set; }

    public int? ParentCategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string CategoryCode { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();

    public virtual ICollection<Exercisecategory> InverseParentCategory { get; set; } = new List<Exercisecategory>();

    public virtual Exercisecategory? ParentCategory { get; set; }
}
