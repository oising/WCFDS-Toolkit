namespace Microsoft.Data.Services.Toolkit.QueryModel
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Extension methods for repository objects.
    /// </summary>
    public static class RepositoryBehaviorExtensions
    {
        /// <summary>
        /// Returns the <see cref="RepositoryBehaviorAttribute"/> based on the
        /// repository <see cref="MethodInfo"/> provided.
        /// </summary>
        /// <param name="repository">The repository object.</param>
        /// <param name="repositoryMethod">The repository method information.</param>
        /// <returns>The proper <see cref="RepositoryBehaviorAttribute"/>.</returns>
        public static RepositoryBehaviorAttribute GetBehavior(this object repository, MethodInfo repositoryMethod)
        {
            if (repositoryMethod == null)
                throw new InvalidOperationException("There is no method named " + repositoryMethod + " on the repository");
            
            var behavior = repositoryMethod.GetAttribute<RepositoryBehaviorAttribute>();

            if (behavior != null)
                return behavior;

            return repository.GetType().GetAttribute<RepositoryBehaviorAttribute>() ?? new RepositoryBehaviorAttribute();
        }

        /// <summary>
        /// Returns the <see cref="RepositoryBehaviorAttribute"/> based on the repository method name provided.
        /// </summary>
        /// <param name="repository">The repository object.</param>
        /// <param name="repositoryMethodName">The repository method name.</param>
        /// <returns>The proper <see cref="RepositoryBehaviorAttribute"/>.</returns>
        public static RepositoryBehaviorAttribute GetBehavior(this object repository, string repositoryMethodName)
        {
            return repository.GetBehavior(repository.GetType().GetMethod(repositoryMethodName));
        }
    }
}