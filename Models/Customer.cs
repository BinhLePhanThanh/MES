public class Customer
{
    public int Id { get; set; }  // Khóa chính

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;  // Mã khách hàng, có thể là mã định danh duy nhất

    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;  // Trạng thái hoạt động của khách hàng

    public string Phone { get; set; } = string.Empty;  // Có thể bao gồm cả máy bàn và di động

    public string Website { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}
