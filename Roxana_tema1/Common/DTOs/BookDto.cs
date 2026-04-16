using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DTOs
{
    public class BookDto
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AuthorName { get; set; }
        public int NumberOfPages { get; set; }
        public float Price { get; set; }
        public Guid? AddedBy { get; set; }
    }
}
