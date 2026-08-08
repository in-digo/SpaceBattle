using Moq;

namespace SpaceBattle.Lib.Tests;

public class UpdateNeighborhoodCommandTests
{
    // Объект, для которого ещё не определена окрестность, добавляется в окрестность, найденную системой
    [Fact]
    public void Execute_ObjectIsNotInNeighborhood_AddsObjectToDetectedNeighborhood()
    {
        var gameObject = new Mock<IUObject>(MockBehavior.Strict);
        var neighborhood = new Neighborhood();
        var neighborhoodSystem = new Mock<INeighborhoodSystem>(MockBehavior.Strict);
        var collisionCommandFactory = new Mock<ICollisionCommandFactory>(MockBehavior.Strict);

        neighborhoodSystem
            .Setup(system => system.GetNeighborhood(gameObject.Object))
            .Returns(neighborhood);

        var nextCommand = new Mock<ICommand>(MockBehavior.Strict);
        nextCommand.Setup(command => command.Execute());

        var command = new UpdateNeighborhoodCommand(
            gameObject.Object,
            neighborhoodSystem.Object,
            collisionCommandFactory.Object,
            nextCommand.Object);

        command.Execute();

        Assert.Contains(gameObject.Object, neighborhood.Objects);

        neighborhoodSystem.Verify(
            system => system.GetNeighborhood(gameObject.Object),
            Times.Once);
    }

    // При переходе в новую окрестность объект удаляется из старой и добавляется в новую
    [Fact]
    public void Execute_ObjectMovedToAnotherNeighborhood_MovesObjectBetweenNeighborhoods()
    {
        var gameObject = new Mock<IUObject>(MockBehavior.Strict);
        var oldNeighborhood = new Neighborhood();
        var newNeighborhood = new Neighborhood();
        var neighborhoodSystem = new Mock<INeighborhoodSystem>(MockBehavior.Strict);
        var collisionCommandFactory = new Mock<ICollisionCommandFactory>(MockBehavior.Strict);

        neighborhoodSystem
            .SetupSequence(system => system.GetNeighborhood(gameObject.Object))
            .Returns(oldNeighborhood)
            .Returns(newNeighborhood);

        var nextCommand = new Mock<ICommand>(MockBehavior.Strict);
        nextCommand.Setup(command => command.Execute());

        var command = new UpdateNeighborhoodCommand(
            gameObject.Object,
            neighborhoodSystem.Object,
            collisionCommandFactory.Object,
            nextCommand.Object);

        command.Execute();
        command.Execute();

        Assert.DoesNotContain(gameObject.Object, oldNeighborhood.Objects);
        Assert.Contains(gameObject.Object, newNeighborhood.Objects);

        neighborhoodSystem.Verify(
            system => system.GetNeighborhood(gameObject.Object),
            Times.Exactly(2));
    }

    // При повторном выполнении в той же окрестности выполняется ранее сформированная макрокоманда проверки коллизий
    [Fact]
    public void Execute_ObjectRemainsInSameNeighborhood_ExecutesCollisionMacroCommand()
    {
        var gameObject = new Mock<IUObject>(MockBehavior.Strict);
        var otherObject = new Mock<IUObject>(MockBehavior.Strict);
        var neighborhood = new Neighborhood();

        neighborhood.Objects.Add(otherObject.Object);

        var neighborhoodSystem = new Mock<INeighborhoodSystem>(MockBehavior.Strict);
        neighborhoodSystem
            .Setup(system => system.GetNeighborhood(gameObject.Object))
            .Returns(neighborhood);

        var collisionCommand = new Mock<ICommand>(MockBehavior.Strict);
        collisionCommand.Setup(command => command.Execute());

        var collisionCommandFactory = new Mock<ICollisionCommandFactory>(MockBehavior.Strict);
        collisionCommandFactory
            .Setup(factory => factory.Create(gameObject.Object, otherObject.Object))
            .Returns(collisionCommand.Object);

        var nextCommand = new Mock<ICommand>(MockBehavior.Strict);
        nextCommand.Setup(command => command.Execute());

        var command = new UpdateNeighborhoodCommand(
            gameObject.Object,
            neighborhoodSystem.Object,
            collisionCommandFactory.Object,
            nextCommand.Object);

        // Первое выполнение помещает объект в окрестность и формирует макрокоманду
        command.Execute();

        // Окрестность не изменилась, поэтому должна выполниться сформированная ранее макрокоманда
        command.Execute();

        collisionCommand.Verify(
            command => command.Execute(),
            Times.Once);

        collisionCommandFactory.Verify(
            factory => factory.Create(
                gameObject.Object,
                otherObject.Object),
            Times.Once);

        neighborhoodSystem.Verify(
            system => system.GetNeighborhood(gameObject.Object),
            Times.Exactly(2));
    }

    // После обработки своей системы окрестностей команда передаёт выполнение следующему звену цепочки
    [Fact]
    public void Execute_Always_PassesExecutionToNextCommand()
    {
        var gameObject = new Mock<IUObject>(MockBehavior.Strict);
        var neighborhood = new Neighborhood();

        var neighborhoodSystem = new Mock<INeighborhoodSystem>(MockBehavior.Strict);
        neighborhoodSystem
            .Setup(system => system.GetNeighborhood(gameObject.Object))
            .Returns(neighborhood);

        var collisionCommandFactory = new Mock<ICollisionCommandFactory>(MockBehavior.Strict);

        var nextCommand = new Mock<ICommand>(MockBehavior.Strict);
        nextCommand.Setup(command => command.Execute());

        var command = new UpdateNeighborhoodCommand(
            gameObject.Object,
            neighborhoodSystem.Object,
            collisionCommandFactory.Object,
            nextCommand.Object);

        // Первое выполнение обрабатывает попадание в окрестность
        command.Execute();

        // Второе выполнение обрабатывает ту же окрестность
        command.Execute();

        nextCommand.Verify(
            command => command.Execute(),
            Times.Exactly(2));

        neighborhoodSystem.Verify(
            system => system.GetNeighborhood(gameObject.Object),
            Times.Exactly(2));
    }

    // Цепочка последовательно обновляет положение объекта в двух разных системах окрестностей
    [Fact]
    public void Execute_ChainContainsTwoNeighborhoodSystems_UpdatesBothNeighborhoods()
    {
        var gameObject = new Mock<IUObject>(MockBehavior.Strict);

        var firstNeighborhood = new Neighborhood();
        var secondNeighborhood = new Neighborhood();

        var firstNeighborhoodSystem = new Mock<INeighborhoodSystem>(MockBehavior.Strict);
        firstNeighborhoodSystem
            .Setup(system => system.GetNeighborhood(gameObject.Object))
            .Returns(firstNeighborhood);

        var secondNeighborhoodSystem = new Mock<INeighborhoodSystem>(MockBehavior.Strict);
        secondNeighborhoodSystem
            .Setup(system => system.GetNeighborhood(gameObject.Object))
            .Returns(secondNeighborhood);

        var collisionCommandFactory = new Mock<ICollisionCommandFactory>(MockBehavior.Strict);

        var endCommand = new Mock<ICommand>(MockBehavior.Strict);
        endCommand.Setup(command => command.Execute());

        var secondCommand = new UpdateNeighborhoodCommand(
            gameObject.Object,
            secondNeighborhoodSystem.Object,
            collisionCommandFactory.Object,
            endCommand.Object);

        var firstCommand = new UpdateNeighborhoodCommand(
            gameObject.Object,
            firstNeighborhoodSystem.Object,
            collisionCommandFactory.Object,
            secondCommand);

        firstCommand.Execute();

        Assert.Contains(
            gameObject.Object,
            firstNeighborhood.Objects);

        Assert.Contains(
            gameObject.Object,
            secondNeighborhood.Objects);

        firstNeighborhoodSystem.Verify(
            system => system.GetNeighborhood(gameObject.Object),
            Times.Once);

        secondNeighborhoodSystem.Verify(
            system => system.GetNeighborhood(gameObject.Object),
            Times.Once);

        endCommand.Verify(
            command => command.Execute(),
            Times.Once);
    }

    // При переходе в новую окрестность старая макрокоманда заменяется макрокомандой для объектов новой окрестности
    [Fact]
    public void Execute_ObjectMovesToAnotherNeighborhood_ReplacesCollisionMacroCommand()
    {
        var gameObject = new Mock<IUObject>(MockBehavior.Strict);
        var oldOtherObject = new Mock<IUObject>(MockBehavior.Strict);
        var newOtherObject = new Mock<IUObject>(MockBehavior.Strict);

        var oldNeighborhood = new Neighborhood();
        oldNeighborhood.Objects.Add(oldOtherObject.Object);

        var newNeighborhood = new Neighborhood();
        newNeighborhood.Objects.Add(newOtherObject.Object);

        var neighborhoodSystem = new Mock<INeighborhoodSystem>(MockBehavior.Strict);
        neighborhoodSystem
            .SetupSequence(system => system.GetNeighborhood(gameObject.Object))
            .Returns(oldNeighborhood)
            .Returns(newNeighborhood)
            .Returns(newNeighborhood);

        var oldCollisionCommand = new Mock<ICommand>(MockBehavior.Strict);
        oldCollisionCommand.Setup(command => command.Execute());

        var newCollisionCommand = new Mock<ICommand>(MockBehavior.Strict);
        newCollisionCommand.Setup(command => command.Execute());

        var collisionCommandFactory = new Mock<ICollisionCommandFactory>(MockBehavior.Strict);

        collisionCommandFactory
            .Setup(factory => factory.Create(gameObject.Object, oldOtherObject.Object))
            .Returns(oldCollisionCommand.Object);

        collisionCommandFactory
            .Setup(factory => factory.Create(gameObject.Object, newOtherObject.Object))
            .Returns(newCollisionCommand.Object);

        var nextCommand = new Mock<ICommand>(MockBehavior.Strict);
        nextCommand.Setup(command => command.Execute());

        var command = new UpdateNeighborhoodCommand(
            gameObject.Object,
            neighborhoodSystem.Object,
            collisionCommandFactory.Object,
            nextCommand.Object);

        // Формируется макрокоманда для старой окрестности
        command.Execute();

        // Объект переходит в новую окрестность, старая макрокоманда заменяется
        command.Execute();

        // Выполняется макрокоманда новой окрестности
        command.Execute();

        Assert.DoesNotContain(
            gameObject.Object,
            oldNeighborhood.Objects);

        Assert.Contains(
            gameObject.Object,
            newNeighborhood.Objects);

        oldCollisionCommand.Verify(
            command => command.Execute(),
            Times.Never);

        newCollisionCommand.Verify(
            command => command.Execute(),
            Times.Once);

        collisionCommandFactory.Verify(
            factory => factory.Create(
                gameObject.Object,
                oldOtherObject.Object),
            Times.Once);

        collisionCommandFactory.Verify(
            factory => factory.Create(
                gameObject.Object,
                newOtherObject.Object),
            Times.Once);

        nextCommand.Verify(
            command => command.Execute(),
            Times.Exactly(3));
    }

    // Для каждого другого объекта окрестности создаётся и выполняется отдельная команда проверки коллизии
    [Fact]
    public void Execute_NeighborhoodContainsSeveralObjects_CreatesAndExecutesCollisionCommandsForEachObject()
    {
        var gameObject = new Mock<IUObject>(MockBehavior.Strict);
        var firstOtherObject = new Mock<IUObject>(MockBehavior.Strict);
        var secondOtherObject = new Mock<IUObject>(MockBehavior.Strict);

        var neighborhoodSystem = new Mock<INeighborhoodSystem>(MockBehavior.Strict);

        var collisionCommandFactory = new Mock<ICollisionCommandFactory>(MockBehavior.Strict);

        var firstCollisionCommand = new Mock<ICommand>(MockBehavior.Strict);
        var secondCollisionCommand = new Mock<ICommand>(MockBehavior.Strict);

        var nextCommand = new Mock<ICommand>(MockBehavior.Strict);

        var neighborhood = new Neighborhood();
        neighborhood.Objects.Add(firstOtherObject.Object);
        neighborhood.Objects.Add(secondOtherObject.Object);

        neighborhoodSystem
            .Setup(system => system.GetNeighborhood(gameObject.Object))
            .Returns(neighborhood);

        collisionCommandFactory
            .Setup(factory => factory.Create(gameObject.Object, firstOtherObject.Object))
            .Returns(firstCollisionCommand.Object);

        collisionCommandFactory
            .Setup(factory => factory.Create(gameObject.Object, secondOtherObject.Object))
            .Returns(secondCollisionCommand.Object);

        firstCollisionCommand.Setup(command => command.Execute());
        secondCollisionCommand.Setup(command => command.Execute());

        nextCommand.Setup(command => command.Execute());

        var command = new UpdateNeighborhoodCommand(
            gameObject.Object,
            neighborhoodSystem.Object,
            collisionCommandFactory.Object,
            nextCommand.Object);

        // Первый запуск создаёт макрокоманду, второй выполняет её в той же окрестности
        command.Execute();
        command.Execute();

        collisionCommandFactory.Verify(
            factory => factory.Create(gameObject.Object, firstOtherObject.Object),
            Times.Once);

        collisionCommandFactory.Verify(
            factory => factory.Create(gameObject.Object, secondOtherObject.Object),
            Times.Once);

        firstCollisionCommand.Verify(
            collisionCommand => collisionCommand.Execute(),
            Times.Once);

        secondCollisionCommand.Verify(
            collisionCommand => collisionCommand.Execute(),
            Times.Once);

        neighborhoodSystem.Verify(
            system => system.GetNeighborhood(gameObject.Object),
            Times.Exactly(2));

        nextCommand.Verify(
            next => next.Execute(),
            Times.Exactly(2));
    }
}