namespace SpaceBattle.Lib;

public interface IFuelable
{
    int FuelReserve { get; set; }
    int FuelConsumption { get; }
}