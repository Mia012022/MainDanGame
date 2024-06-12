using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class App
{
    public int AppId { get; set; }

    public string AppName { get; set; } = null!;

    public virtual AppDetail? AppDetail { get; set; }

    public virtual ICollection<AppMedium> AppMedia { get; set; } = new List<AppMedium>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

    public virtual ICollection<App> Apps { get; set; } = new List<App>();

    public virtual ICollection<App> Dlcapps { get; set; } = new List<App>();

    public virtual ICollection<GenreTag> Tags { get; set; } = new List<GenreTag>();
}
