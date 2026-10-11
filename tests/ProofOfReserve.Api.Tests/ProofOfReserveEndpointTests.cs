using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;

using Sakura.ProofOfReserve.Api.Endpoints;

namespace Sakura.ProofOfReserve.Api.Tests;

public sealed class ProofOfReserveEndpointTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private readonly StringContent _userDataSet = new(
        """
        {
            "users": [
                { "id": 1, "balance": 1111 },
                { "id": 2, "balance": 2222 },
                { "id": 3, "balance": 3333 },
                { "id": 4, "balance": 4444 },
                { "id": 5, "balance": 5555 },
                { "id": 6, "balance": 6666 },
                { "id": 7, "balance": 7777 },
                { "id": 8, "balance": 8888 }
            ]
        }
        """,
        Encoding.UTF8,
        "application/json"
    );

    private const string UserEndpoint = UserEndpoints.BasePath;

    public ProofOfReserveEndpointTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    // We explicitely want a fresh database between each test.
    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Get_ProofOfFundsForAnExistingUser_ReturnsOk()
    {
        HttpResponseMessage response = await _client.PostAsync(
            UserEndpoint,
            _userDataSet,
            TestContext.Current.CancellationToken);

        // Users are created
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        HttpResponseMessage proofOfReserveResponse = await _client.GetAsync(
            $"{UserEndpoint}/1",
            TestContext.Current.CancellationToken);

        var content = await proofOfReserveResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, proofOfReserveResponse.StatusCode);
        Assert.Equal(
            "{\"merkleProof\":{\"userBalance\":1111,\"proofMetadata\":[{\"siblingHash\":\"04bd4a356d675cc13ea5b0fc83e0736a3fbf3067980de9e8e0553c934f5906b8\",\"siblingPosition\":1},{\"siblingHash\":\"d185af244042b0fecba7ee16c9933d73b10c5482104538274dd777b6b120eae1\",\"siblingPosition\":1},{\"siblingHash\":\"9d7f79fa8e788d4a32c9c674b67dcfaf0885f539ac2699129e3c4d88c11c76e7\",\"siblingPosition\":1}]}}",
            content
        );
    }

    [Fact]
    public async Task Get_ProofOfFundsForANonExistingUser_ReturnsNotFound()
    {
        HttpResponseMessage response = await _client.GetAsync(
            $"{UserEndpoint}/1",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreateUsers_ReturnsMerkleRootForAllUsers()
    {
        HttpResponseMessage response = await _client.PostAsync(
            UserEndpoint,
            _userDataSet,
            TestContext.Current.CancellationToken);

        var rawResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"merkleRoot\":\"b1231de33da17c23cebd80c104b88198e0914b0463d0e14db163605b904a7ba3\"}", rawResponse);
    }

    [Fact]
    public async Task Post_CreateSameUsers_ReturnsNotFound()
    {
        var content = new StringContent(
            """
            {
                "users": [
                    { "id": 1, "balance": 1111 },
                    { "id": 2, "balance": 2222 }
                ]
            }
            """,
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response = await _client.PostAsync(
            UserEndpoint, content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // First creation of users succeeded, we move on to the second one.

        HttpResponseMessage retriedResponse = await _client.PostAsync(
            UserEndpoint, content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, retriedResponse.StatusCode);
    }
    
    [Fact]
    public async Task Post_CreateDifferentUsersInTwoBatch_ReturnsOk()
    {
        var content = new StringContent(
            """
            {
                "users": [
                    { "id": 1, "balance": 1111 },
                    { "id": 2, "balance": 2222 }
                ]
            }
            """,
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response = await _client.PostAsync(
            UserEndpoint, content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // First creation of users succeeded, we move on to the second one.

        content = new StringContent(
            """
            {
                "users": [
                    { "id": 3, "balance": 3 },
                    { "id": 4444, "balance": 4444 }
                ]
            }
            """,
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage retriedResponse = await _client.PostAsync(
            UserEndpoint, content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, retriedResponse.StatusCode);
    }

}
