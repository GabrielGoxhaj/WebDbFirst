namespace WebDbFirst.Models
{
    public class CustomerDto
    {
        /// <summary>
        /// Primary key for Customer records.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// 0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.
        /// </summary>
        public bool NameStyle { get; set; }

        /// <summary>
        /// A courtesy title. For example, Mr. or Ms.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// First name of the person.
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Middle name or middle initial of the person.
        /// </summary>
        public string? MiddleName { get; set; }

        /// <summary>
        /// Last name of the person.
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Surname suffix. For example, Sr. or Jr.
        /// </summary>
        public string? Suffix { get; set; }

        /// <summary>
        /// The customer&apos;s organization.
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// The customer&apos;s sales person, an employee of AdventureWorks Cycles.
        /// </summary>
        public string? SalesPerson { get; set; }

        public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; } = new List<SalesOrderHeader>();

    }
}
