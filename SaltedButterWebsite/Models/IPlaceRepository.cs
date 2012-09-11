using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaltedButterWebsite.Models
{
    public interface IPlaceRepository
    {
        IList<Place> GetAll();

        IList<Place> GetById(int id);

        IList<Place> Update(Place place);

        IList<Place> Remove(Place place);

        IList<Place> Create(Place place);
    }
}
