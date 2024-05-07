import { useEffect, useState, useMemo, useCallback } from "react";
import { useParams } from "react-router-dom";

import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";
import { ProductModel } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { ShowFor } from "../../Shared/ShowFor";

export const Products = () => {
  const [products, setProducts] = useState<ProductModel[]>();
  const { cluster } = useParams();

  useEffect(() => {
    const fetchProducts = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/product/index`,
      );

      if (response.ok) {
        const data = await response.json();
        setProducts(data);
      } else {
        alert("Error fetching groups");
      }
    };

    fetchProducts();
  }, [cluster]);

  const columns = useMemo<Column<ProductModel>[]>(
    () => [
      {
        Header: "Name",
        accessor: "name",
        Cell: ({ row }) => (
          <a href={`/${cluster}/product/${row.original.id}`}>
            {row.original.name}
          </a>
        ),
      },
      {
        Header: "Category",
        accessor: "category",
      },
      {
        Header: "Description",
        accessor: "description",
      },
      {
        Header: "Unit Price",
        accessor: "unitPrice",
      },
      {
        Header: "Units",
        accessor: "units",
      },
      {
        Header: "Installments",
        accessor: "installments",
      },
      {
        Header: "Actions",
        accessor: "id",
        Cell: ({ row }) => (
          <div>
            <button className="btn btn-primary">Order</button>{" "}
            <ShowFor roles={["ClusterAdmin"]}>
              <button className="btn btn-primary">Edit</button>{" "}
              <button className="btn btn-danger">Delete</button>
            </ShowFor>
          </div>
        ),
      },
    ],
    [cluster],
  );

  if (products === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    return (
      <div>
        <ShowFor roles={["ClusterAdmin"]}>
          <div className="row justify-content-center">
            <div className="col-md-8">
              <button className="btn btn-primary"> Add Product </button>{" "}
              <button className="btn btn-primary"> Adhoc Order </button>
            </div>
          </div>
        </ShowFor>
        <hr />
        <div className="row justify-content-center">
          <div className="col-md-8">
            <ReactTable
              columns={columns}
              data={products}
              initialState={{
                sortBy: [{ id: "Name" }],
              }}
            />
          </div>
        </div>
      </div>
    );
  }
};
