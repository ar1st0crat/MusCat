using MusCat.ViewModels.Entities;
using Prism.Events;

namespace MusCat.Events
{
    class AlbumRateUpdatedEvent : PubSubEvent<AlbumViewModel>
    {
    }
}
