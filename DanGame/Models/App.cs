using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DanGame.Models;

public partial class App
{
    public int AppId { get; set; }

    public string AppName { get; set; } = null!;

    [JsonIgnore]
    public virtual AppDetail? AppDetail { get; set; }
    [JsonIgnore]

    public virtual ICollection<AppMedium> AppMedia { get; set; } = new List<AppMedium>();
    [JsonIgnore]

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    [JsonIgnore]

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
    [JsonIgnore]

    public virtual ICollection<App> Apps { get; set; } = new List<App>();
    [JsonIgnore]

    public virtual ICollection<App> Dlcapps { get; set; } = new List<App>();
    [JsonIgnore]

    public virtual ICollection<GenreTag> Tags { get; set; } = new List<GenreTag>();
}
