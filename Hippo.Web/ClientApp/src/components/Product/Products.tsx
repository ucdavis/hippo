import { useEffect, useState, useMemo, useCallback } from "react";
import { Link, useParams } from "react-router-dom";

import { HipTable } from "../../Shared/Table/HipTable";
import { createColumnHelper } from "@tanstack/react-table";
import { ProductModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { ShowFor } from "../../Shared/ShowFor";
import { usePromiseNotification } from "../../util/Notifications";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { notEmptyOrFalsey } from "../../util/ValueChecks";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipBody from "../../Shared/Layout/HipBody";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";

const defaultProduct: ProductModel = {
  id: 0,
  name: "",
  category: "Memory",
  description: "",
  unitPrice: "0.00",
  units: "",
  installments: 1,
  installmentType: "OneTime",
  lifeCycle: 60,
};

export const Products = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [editProductModel, setEditProductModel] = useState<ProductModel>({
    ...defaultProduct,
  });
  const [editConfirmationTitle, setEditConfirmationTitle] = useState("");
  const [deleteConfirmationTitle, setDeleteConfirmationTitle] =
    useState("Delete Product");
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
        alert("Error fetching products");
      }
    };

    fetchProducts();
  }, [cluster]);

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
              maxLength={50}
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
            <textarea
              className="form-control"
              id="fieldDescription"
              value={editProductModel.description}
              required
              maxLength={250}
              rows={3}
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
              maxLength={50}
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
              maxLength={50}
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
            <label htmlFor="fieldInstallmentType">Installment Type</label>
            <select
              className="form-control"
              id="fieldInstallmentType"
              required
              value={editProductModel.installmentType}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  installmentType: e.target.value,
                };
                if (model.installmentType === "OneTime") {
                  model.installments = 1;
                }
                if (
                  model.installmentType === "Monthly" &&
                  (model.installments === 1 || model.installments === 5)
                ) {
                  model.installments = 60;
                }
                if (
                  model.installmentType === "Yearly" &&
                  (model.installments === 1 || model.installments === 60)
                ) {
                  model.installments = 5;
                }
                setEditProductModel(model);
                setReturn(model);
              }}
            >
              <option value="OneTime">One Time</option>
              <option value="Monthly">Monthly</option>
              <option value="Yearly">Yearly</option>
            </select>
          </div>
          {editProductModel.installmentType === "OneTime" ? null : (
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
          )}
          <div className="form-group">
            <label htmlFor="lifeCycle">Life Cycle in Months</label>
            <input
              className="form-control"
              id="lifeCycle"
              required
              value={editProductModel.lifeCycle}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  lifeCycle: parseInt(e.target.value),
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
        notEmptyOrFalsey(editProductModel.category) &&
        notEmptyOrFalsey(editProductModel.units) &&
        notEmptyOrFalsey(editProductModel.description) &&
        !notification.pending &&
        parseFloat(editProductModel.unitPrice) > 0 &&
        editProductModel.installments > 0 &&
        editProductModel.lifeCycle > 0,
    },
    [editProductModel, notification.pending],
  );

  const [getDeleteConfirmation] = useConfirmationDialog<string>(
    {
      title: deleteConfirmationTitle,
      message:
        "Are you sure you want to delete this product? This does not effect any orders.",
    },
    [deleteConfirmationTitle],
  );

  const handleDelete = useCallback(
    async (id: number) => {
      const product = products?.find((p) => p.id === id);

      if (product === undefined) {
        alert("Product not found");
        return;
      }

      setDeleteConfirmationTitle(`Delete Product "${product.name}"?`);

      const [confirmed] = await getDeleteConfirmation();

      if (!confirmed) {
        return;
      }

      const req = authenticatedFetch(
        `/api/${cluster}/product/DeleteProduct/${id}`,
        {
          method: "POST",
        },
      );

      setNotification(req, "Deleting", "Product Deleted", async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await req;

      if (response.ok) {
        setProducts(products?.filter((p) => p.id !== id));
      }
    },
    [products, getDeleteConfirmation, cluster, setNotification],
  );

  const handleEdit = useCallback(
    async (id: number) => {
      const product = products?.find((p) => p.id === id);
      if (product === undefined) {
        alert("Product not found");
        return;
      }

      setEditProductModel(product);
      setEditConfirmationTitle("Edit Product");
      const [confirmed, newModel] = await getEditConfirmation();

      if (!confirmed) {
        return;
      }

      const req = authenticatedFetch(`/api/${cluster}/product/UpdateProduct/`, {
        method: "POST",
        body: JSON.stringify(newModel),
      });

      setNotification(req, "Saving", "Product Updated", async (r) => {
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
        setProducts(
          products?.map((p) => {
            if (p.id === id) {
              return data;
            } else {
              return p;
            }
          }),
        );
      }
    },
    [products, getEditConfirmation, cluster, setNotification],
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

  const columnHelper = createColumnHelper<ProductModel>();

  const columns = [
    columnHelper.accessor("name", {
      header: "Name",
      id: "name",
    }),
    columnHelper.accessor("category", {
      header: "Category",
      id: "category",
    }),
    columnHelper.accessor("description", {
      header: "Description",
      id: "description",
    }),
    columnHelper.accessor("unitPrice", {
      header: "Unit Price",
      id: "unitPrice",
    }),
    columnHelper.accessor("units", {
      header: "Units",
      id: "units",
    }),
    columnHelper.accessor("installments", {
      header: "Installments",
      id: "installments",
    }),
    columnHelper.accessor("installmentType", {
      header: "Type",
      id: "installmentType",
    }),
    columnHelper.display({
      id: "actions",
      header: "Actions",
      cell: ({ row }) => (
        <div>
          <Link
            className="btn btn-primary"
            to={`/${cluster}/order/create/${row.original.id}`}
          >
            Order
          </Link>{" "}
          <ShowFor roles={["ClusterAdmin"]}>
            <button
              onClick={() => handleEdit(row.original.id)}
              className="btn btn-primary"
            >
              Edit
            </button>{" "}
            <button
              onClick={() => handleDelete(row.original.id)}
              className="btn btn-danger"
            >
              Delete
            </button>
          </ShowFor>
        </div>
      ),
    }),
  ];

  if (products === undefined) {
    // RH TODO: suspense/error boundaries
    return (
      <HipMainWrapper>
        <HipTitle title="Products" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
      <HipMainWrapper>
        <HipTitle
          title="Products"
          buttons={
            <ShowFor roles={["ClusterAdmin"]}>
              <>
                <button className="btn btn-primary" onClick={handleCreate}>
                  {" "}
                  Add Product{" "}
                </button>{" "}
                <Link
                  className="btn btn-primary"
                  to={`/${cluster}/order/create`}
                >
                  {" "}
                  Adhoc Order{" "}
                </Link>{" "}
              </>
            </ShowFor>
          }
        />
        <HipBody>
          <HipTable columns={columns} data={products} />
        </HipBody>
      </HipMainWrapper>
    );
  }
};
