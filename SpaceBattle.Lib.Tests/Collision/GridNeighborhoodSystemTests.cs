using Moq;

namespace SpaceBattle.Lib.Tests;

public class GridNeighborhoodSystemTests
{
    // Объекты из одной ячейки сетки получают один и тот же экземпляр окрестности
    [Fact]
    public void GetNeighborhood_ObjectsBelongToSameCell_ReturnsSameNeighborhood()
    {
        var firstObject = new Mock<IUObject>(MockBehavior.Strict);
        firstObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(1, 2));

        var secondObject = new Mock<IUObject>(MockBehavior.Strict);
        secondObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(9, 8));

        var system = new GridNeighborhoodSystem(
            neighborhoodSize: 10,
            offset: new Vector(0, 0));

        var firstNeighborhood = system.GetNeighborhood(firstObject.Object);
        var secondNeighborhood = system.GetNeighborhood(secondObject.Object);

        Assert.Same(firstNeighborhood, secondNeighborhood);
    }

    // Объекты, различающиеся по третьей координате, могут находиться в разных окрестностях
    [Fact]
    public void GetNeighborhood_ObjectsDifferByThirdCoordinate_ReturnsDifferentNeighborhoods()
    {
        var firstObject = new Mock<IUObject>(MockBehavior.Strict);
        firstObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(1, 2, 1));

        var secondObject = new Mock<IUObject>(MockBehavior.Strict);
        secondObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(1, 2, 11));

        var system = new GridNeighborhoodSystem(
            neighborhoodSize: 10,
            offset: new Vector(0, 0, 0));

        var firstNeighborhood = system.GetNeighborhood(firstObject.Object);
        var secondNeighborhood = system.GetNeighborhood(secondObject.Object);

        Assert.NotSame(
            firstNeighborhood,
            secondNeighborhood);
    }

    // Объекты по разные стороны границы обычной сетки попадают в одну окрестность смещённой сетки
    [Fact]
    public void GetNeighborhood_GridIsShifted_GroupsObjectsAcrossRegularGridBoundary()
    {
        var firstObject = new Mock<IUObject>(MockBehavior.Strict);
        firstObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(9, 9));

        var secondObject = new Mock<IUObject>(MockBehavior.Strict);
        secondObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(11, 11));

        var regularSystem = new GridNeighborhoodSystem(
            neighborhoodSize: 10,
            offset: new Vector(0, 0));

        var shiftedSystem = new GridNeighborhoodSystem(
            neighborhoodSize: 10,
            offset: new Vector(5, 5));

        var firstRegularNeighborhood = regularSystem.GetNeighborhood(firstObject.Object);
        var secondRegularNeighborhood = regularSystem.GetNeighborhood(secondObject.Object);

        var firstShiftedNeighborhood = shiftedSystem.GetNeighborhood(firstObject.Object);
        var secondShiftedNeighborhood = shiftedSystem.GetNeighborhood(secondObject.Object);

        Assert.NotSame(
            firstRegularNeighborhood,
            secondRegularNeighborhood);

        Assert.Same(
            firstShiftedNeighborhood,
            secondShiftedNeighborhood);
    }

    // Размер окрестности должен быть положительным
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NeighborhoodSizeIsNotPositive_ThrowsArgumentOutOfRangeException(int neighborhoodSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new GridNeighborhoodSystem(neighborhoodSize, new Vector(0, 0)));
    }

    // Размерность позиции должна совпадать с размерностью смещения сетки
    [Fact]
    public void GetNeighborhood_PositionAndOffsetHaveDifferentDimensions_ThrowsArgumentException()
    {
        var gameObject = new Mock<IUObject>(MockBehavior.Strict);
        gameObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(1, 2, 3));

        var system = new GridNeighborhoodSystem(
            neighborhoodSize: 10,
            offset: new Vector(0, 0));

        Assert.Throws<ArgumentException>(() =>
            system.GetNeighborhood(gameObject.Object));
    }

    // Объекты по разные стороны от нуля находятся в разных окрестностях
    [Fact]
    public void GetNeighborhood_ObjectsAreOnDifferentSidesOfZero_ReturnsDifferentNeighborhoods()
    {
        var negativePositionObject = new Mock<IUObject>(MockBehavior.Strict);
        negativePositionObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(-1, -1));

        var positivePositionObject = new Mock<IUObject>(MockBehavior.Strict);
        positivePositionObject
            .Setup(gameObject => gameObject.GetProperty("Position"))
            .Returns(new Vector(1, 1));

        var system = new GridNeighborhoodSystem(
            neighborhoodSize: 10,
            offset: new Vector(0, 0));

        var negativeNeighborhood = system.GetNeighborhood(negativePositionObject.Object);
        var positiveNeighborhood = system.GetNeighborhood(positivePositionObject.Object);

        Assert.NotSame(
            negativeNeighborhood,
            positiveNeighborhood);
    }
}