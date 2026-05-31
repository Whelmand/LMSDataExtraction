using LMSDataExtraction.Domain.Entities;
using Xunit;

namespace LMSDataExtraction.Tests.Entities;

public class UserTests
{
    [Fact]
    public void NewUser_ShouldHaveEmptyDefaults()
    {
        User user = new User();

        Assert.Equal(0, user.Id);
        Assert.Equal(0, user.CanvasId);
        Assert.Equal(string.Empty, user.Name);
        Assert.Equal(string.Empty, user.Email);
        Assert.Equal(string.Empty, user.Role);
        Assert.NotNull(user.Activities);
        Assert.Empty(user.Activities);
    }
}
