using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public enum ConnectionEndpoint
    {
        stocks = 0,
        options = 2
    }
    public enum PolygonTimespan
    {
        second = 0,
        minute = 1,
        hour = 2,
        day = 3,
        week = 4,
        month = 5,
        quarter = 6,
        year = 7
    }
    public enum PolygonSort
    {
        asc = 0,
        desc = 1
    }
    public enum PolygonOrder
    {
        asc = 0,
        desc = 1
    }
    public enum PolygonFilterParams
    {
        none = 0,
        gt = 1,
        gte = 2,
        lt = 3,
        lte = 4
    }
}
