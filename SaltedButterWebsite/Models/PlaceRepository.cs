using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaltedButterWebsite.Models
{
    public class PlaceRepository : IPlaceRepository
    {
        private PlaceSatedButterDataContext _dataContext;

        public PlaceRepository()
        {
            this._dataContext = new PlaceSatedButterDataContext();
        }

        public IList<Place> GetAll()
        {
            return null;
            
        }

        public IList<Place> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IList<Place> Update(Place place)
        {
            throw new NotImplementedException();
        }

        public IList<Place> Remove(Place place)
        {
            throw new NotImplementedException();
        }

        public IList<Place> Create(Place place)
        {
            throw new NotImplementedException();
        }
    }
}
