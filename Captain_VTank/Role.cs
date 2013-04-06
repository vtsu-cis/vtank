namespace Captain_VTank
{
    /// <summary>
    /// Represents a user's account privilege. Roles are sorted by least privileged to most privileged.
    /// </summary>
    public enum Role
    {
        Offline = -100,
        Banned = -99,
        Suspended = -2,
        Troublemaker = -1,
        Member = 0,
        Contributor = 1,
        Tester = 2,
        Developer = 5,
        Moderator = 10,
        Administrator = 99,
    }

    /// <summary>
    /// Provides static utility methods for the Role enumeration.
    /// </summary>
    public class RoleUtil
    {
        /// <summary>
        /// Get the Role enumeration based on the ID integer.
        /// </summary>
        /// <param name="id">ID associated to the role.</param>
        /// <returns>Role object.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the ID is not in the Role enumeration.</exception>
        public static Role GetRole(int id)
        {
            return (Role)System.Enum.Parse(typeof(Role), id.ToString());
        }
    }
}
