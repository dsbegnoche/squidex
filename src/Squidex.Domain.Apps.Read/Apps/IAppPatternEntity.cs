/*
 * CivicPlus implementation of Squidex Headless CMS
 */

namespace Squidex.Domain.Apps.Read.Apps
{
    public interface IAppPatternEntity
    {
        string Name { get; }
        string Pattern { get; }
        string DefaultMessage { get; }
    }
}
