using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using System.Linq.Expressions;
using DataService.Data;
using DataService.Entities;
using DataService.DTOs;
using WebService.Controllers;

namespace WebService.Tests.Unit.Controllers;

public class IndividualControllerUnitTests
{
    private readonly Mock<CITContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly IndividualController _controller;

    public IndividualControllerUnitTests()
    {
        _mockContext = new Mock<CITContext>();
        _mockMapper = new Mock<IMapper>();
        _controller = new IndividualController(_mockContext.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetIndividual_ExistingId_ReturnsOkWithIndividual()
    {
        // Arrange
        var testId = "nm0000001";
        var individual = new Individual { Iconst = testId, Name = "Tom Hanks" };
        var expectedDto = new IndividualFullDTO { Id = testId, Name = "Tom Hanks" };

        // Mock database call
        var mockDbSet = CreateMockDbSet(new[] { individual });
        _mockContext.Setup(c => c.Individuals).Returns(mockDbSet.Object);

        // Mock mapper
        _mockMapper.Setup(m => m.Map<IndividualFullDTO>(individual))
                   .Returns(expectedDto);

        // Act
        var result = await _controller.GetIndividual(testId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<IndividualFullDTO>(okResult.Value);
        Assert.Equal(testId, returnedDto.Id);
        Assert.Equal("Tom Hanks", returnedDto.Name);
    }

    [Fact]
    public async Task GetIndividual_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var testId = "nm9999999";
        var mockDbSet = CreateMockDbSet(new Individual[0]); // Empty collection
        _mockContext.Setup(c => c.Individuals).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.GetIndividual(testId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Individual not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task Search_ValidName_ReturnsMatchingIndividuals()
    {
        // Arrange
        var searchName = "Tom";
        var individuals = new[]
        {
            new Individual { Iconst = "nm0000001", Name = "Tom Hanks" },
            new Individual { Iconst = "nm0000002", Name = "Tom Cruise" }
        };

        var mockDbSet = CreateMockDbSet(individuals);
        _mockContext.Setup(c => c.Individuals).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.Search(searchName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<IndividualReferenceDTO>>(okResult.Value);
        Assert.Equal(2, returnedList.Count);
    }

    [Fact]
    public async Task Search_EmptyName_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.Search("");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedList = Assert.IsType<List<IndividualReferenceDTO>>(okResult.Value);
        Assert.Empty(returnedList);
    }

    // Helper method to create mock DbSet
    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> elements) where T : class
    {
        var elementsAsQueryable = elements.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();

        dbSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(elementsAsQueryable.GetEnumerator()));

        dbSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(elementsAsQueryable.Provider));

        dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elementsAsQueryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elementsAsQueryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(elementsAsQueryable.GetEnumerator());

        return dbSet;
    }
}

// Helper classes for async testing
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        return Execute<TResult>(expression);
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider
    {
        get { return new TestAsyncQueryProvider<T>(this); }
    }
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current
    {
        get
        {
            return _inner.Current;
        }
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}