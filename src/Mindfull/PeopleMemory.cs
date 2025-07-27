using Microsoft.KernelMemory;
using System.Threading;
using System.Threading.Tasks;

namespace Mindfull;


/// <summary>
/// Class which uses KernelMemory to store and retrieve information about people the user knows
/// It does this by storing documents in memory where each document represents a single person
/// and information known about them.  As the user interacts with the system, it will update the documents.
/// </summary>
public class PeopleMemory
{
    private readonly IKernelMemory _kernelMemory;
    private readonly string _index;

    /// <summary>
    /// The userId for the memory is for.
    /// </summary>
    /// <param name="kernelMemory">the kernel memory configuration to use.</param>
    /// <param name="userId">the unique id identifying the user for this memory</param>
    public PeopleMemory(IKernelMemory kernelMemory, string userId)
    {
        _kernelMemory = kernelMemory;
        this._index = userId;
    }

    /// <summary>
    /// Resolve a person to an id based on their reference.
    /// </summary>
    /// <param name="reference">The reference to search for, e.g. a name or description of someone relatated to the user.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>the unique personid for the resolved person.</returns>
    public async Task<string> ResolvePersonIdAsync(string reference, CancellationToken cancellationToken)
    {
        // SearchAsync returns a SearchResult object, not an async enumerable
        var result = await _kernelMemory.SearchAsync(query: reference, index: _index, limit: 1, cancellationToken: cancellationToken);
        if (result.Results != null && result.Results.Count > 0)
        {
            return result.Results[0].DocumentId;
        }
        return string.Empty;
    }

    /// <summary>
    /// Answer a question about a person based on their id or reference.
    /// </summary>
    public async Task<MemoryAnswer> AskAsync(string personId, string question, CancellationToken cancellationToken)
    {
        var result = await _kernelMemory.AskAsync(question, index: _index, filter: MemoryFilters.ByDocument(personId),  cancellationToken: cancellationToken);
        return result;
    }

    /// <summary>
    /// Add or update a person in the system
    /// </summary>
    public async Task AddOrUpdatePersonAsync(string personId, string textAboutPerson, CancellationToken cancellationToken)
    {
        // ImportTextAsync(string text, string? documentId = null, string? fileName = null, string? collection = null, CancellationToken cancellationToken = default)
        await _kernelMemory.ImportTextAsync(textAboutPerson, personId, null, index: _index, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Delete a person from the system by their id.
    /// </summary>
    public async Task DeletePersonAsync(string personId, CancellationToken cancellationToken)
    {
        // RemoveDocumentAsync(string collection, string documentId, CancellationToken cancellationToken = default)
        await _kernelMemory.RemoveDocumentAsync(_index, personId, cancellationToken: cancellationToken);
    }
}
