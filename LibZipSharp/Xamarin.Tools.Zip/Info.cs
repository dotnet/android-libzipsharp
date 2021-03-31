namespace Xamarin.Tools.Zip
{
	public class Info
	{
		public static Versions GetVersions ()
		{
			return Native.get_versions ();
		}
	}
}
