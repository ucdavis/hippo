import { useEffect, useState, useMemo, useCallback, useContext } from "react";
import { useParams } from "react-router-dom";

import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";
import { ProductModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { ShowFor } from "../../Shared/ShowFor";
import { usePromiseNotification } from "../../util/Notifications";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { notEmptyOrFalsey } from "../../util/ValueChecks";

const defaultProduct: ProductModel = {
  id: 0,
  name: "",
  category: "Memory",
  description: "",
  unitPrice: "0.00",
  units: "",
  installments: 60,
};

export const Products = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [editProductModel, setEditProductModel] = useState<ProductModel>({
    ...defaultProduct,
  });
  const [editConfirmationTitle, setEditConfirmationTitle] = useState("");
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

  const [getEditConfirmation] = useConfirmationDialog<ProductModel>(
    {
      title: editConfirmationTitle,
      message: (setReturn) => (
        <>
          <div className="form-group">
            <label htmlFor="fieldName">Name</label>
            <input
              className="form-control"
              id="fieldName"
              required
              value={editProductModel.name}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  name: e.target.value,
                };
                setEditProductModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldDescription">Description</label>
            <input
              className="form-control"
              id="fieldDescription"
              value={editProductModel.description}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  description: e.target.value,
                };
                setEditProductModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldCategory">Category</label>
            <input
              className="form-control"
              id="fieldCategory"
              required
              value={editProductModel.category}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  category: e.target.value,
                };
                setEditProductModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldUnitPrice">Unit Price</label>
            <input
              className="form-control"
              id="fieldUnitPrice"
              type="decimal"
              required
              value={editProductModel.unitPrice}
              onChange={(e) => {
                const value = e.target.value;
                if (/^\d*\.?\d*$/.test(value) || /^\d*\.$/.test(value)) {
                  // This regex checks for a valid decimal or integer
                  const model: ProductModel = {
                    ...editProductModel,
                    unitPrice: value,
                  };
                  setEditProductModel(model);
                  setReturn(model);
                }
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldUnits">Units</label>
            <input
              className="form-control"
              id="fieldUnits"
              required
              value={editProductModel.units}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  units: e.target.value,
                };
                setEditProductModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldInstallments">Installments</label>
            <input
              className="form-control"
              id="fieldInstallments"
              required
              value={editProductModel.installments}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  installments: parseInt(e.target.value),
                };
                setEditProductModel(model);
                setReturn(model);
              }}
            />
          </div>
        </>
      ),
      canConfirm:
        notEmptyOrFalsey(editProductModel.name) &&
        !notification.pending &&
        parseFloat(editProductModel.unitPrice) > 0 &&
        editProductModel.installments > 0,
    },
    [],
  );

  const handleCreate = async () => {
    setEditProductModel({ ...defaultProduct });
    setEditConfirmationTitle("Add Product");
    const [confirmed, newModel] = await getEditConfirmation();

    if (!confirmed) {
      return;
    }

    const req = authenticatedFetch(`/api/${cluster}/product/CreateProduct`, {
      method: "POST",
      body: JSON.stringify(newModel),
    });

    setNotification(req, "Saving", "Product Added", async (r) => {
      if (r.status === 400) {
        const errors = await parseBadRequest(response);
        return errors;
      } else {
        return "An error happened, please try again.";
      }
    });

    const response = await req;

    if (response.ok) {
      const data = await response.json();
      //add data to the projects
      setProducts([...products, data]);
    }
  };

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
              <button className="btn btn-primary" onClick={handleCreate}>
                {" "}
                Add Product{" "}
              </button>{" "}
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
