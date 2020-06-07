using System.Collections.Generic;
using System.Linq;

namespace ELK.Demo
{
	public class Data
	{
		public static IEnumerable<AuthorBook> AuthorBooks;

		public static void Seed()
		{
			AuthorBooks = Enumerable
				.Repeat<int>(default, 1_000)
				.Select((_,
					index) =>
				{
					var temp = ++index;
					return new AuthorBook
					{

						Id = temp,
						Name = $"Huy_{temp}",
						BookName = $"Book_{temp}"
					};
				});
		}
	}
}