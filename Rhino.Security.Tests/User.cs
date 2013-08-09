namespace Rhino.Security.Tests
{
    public class User : IUser
    {
        public static bool DisableEqualityOverrides = false;

        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the security info for this user
        /// </summary>
        /// <value>The security info.</value>
        public virtual SecurityInfo SecurityInfo
        {
            get { return new SecurityInfo(Name, Id); }
        }

        public virtual bool Equals(User other)
        {
            if (DisableEqualityOverrides)
            {
                return base.Equals(other);
            }

            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (DisableEqualityOverrides)
            {
                return base.Equals(obj);
            }

            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(User)) return false;
            return Equals((User)obj);
        }

        public override int GetHashCode()
        {
            if (DisableEqualityOverrides)
            {
                return base.GetHashCode();
            }

            return Id.GetHashCode();
        }
    }
}