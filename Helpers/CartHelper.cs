using PetShop.Models;
using System.Text.Json;

namespace PetShop.Helpers
{
    public static class CartHelper
    {
        private const string CartKey = "Cart";

        public static List<CartItem> GetCart(ISession session)
        {
            var json = session.GetString(CartKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(json)
                   ?? new List<CartItem>();
        }

        public static void SaveCart(ISession session, List<CartItem> cart)
        {
            session.SetString(CartKey, JsonSerializer.Serialize(cart));
        }

        public static void ClearCart(ISession session)
        {
            session.Remove(CartKey);
        }

        public static int GetCartCount(ISession session)
        {
            return GetCart(session).Sum(x => x.SoLuong);
        }
    }
}