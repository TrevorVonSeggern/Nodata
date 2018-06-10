# NOData
Not only data, but a queryable interface! [![pipeline status](https://gitlab.com/tvo/Nodata/badges/master/pipeline.svg)](https://gitlab.com/tvo/Nodata/commits/master) [![coverage report](https://gitlab.com/tvo/Nodata/badges/master/coverage.svg)](https://gitlab.com/tvo/Nodata/commits/master)

NOData is a loose implementation of the OData [specification](http://docs.oasis-open.org/odata/odata/v4.0/errata03/os/complete/part2-url-conventions/odata-v4.0-errata03-os-part2-url-conventions-complete.html).

With this package, you can perform queries like the following:
/products

/products$where=id eq 10

/products$where=id ne 10

/products$where=id lt 3 and (Name eq 'Bob' or Name eq 'Rob')

/products$expand=Role

/products$expand=Role,Manager

/products$expand=Role, Manager, Employees

/products$expand=Role,Manager, Employees & where Role/Name eq 'Admin' or Manager/id eq 1

/products$expand=Role($select=Name,id)

Check out the wikki for more information:
https://gitlab.com/tvo/Nodata/wikis/home
