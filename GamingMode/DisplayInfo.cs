namespace Model;

public struct DisplayInfo
{
    public required string Display { get; init; }
    public required Resolution Resolution { get; init; }
    public required int RefreshRate { get; init; }
}

public struct Resolution
{
    public required int Width { get; init; }
    public required int Height { get; init; }
}