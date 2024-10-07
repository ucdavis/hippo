import { useEffect, useState, useCallback } from "react";
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
import HipButton from "../../Shared/HipComponents/HipButton";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";
import HipLoadingTable from "../../Shared/LoadingAndErrors/HipLoadingTable";

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
  isRecurring: false,
  isUnavailable: false,
  isHiddenFromPublic: false,
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
              defaultValue={editProductModel.name}
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
              defaultValue={editProductModel.description}
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
              defaultValue={editProductModel.category}
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
              defaultValue={editProductModel.unitPrice}
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
              defaultValue={editProductModel.units}
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
            <label htmlFor="fieldIsUnavailable">Is Unavailable</label>{" "}
            <input
              id="fieldIsUnavailable"
              type="checkbox"
              checked={editProductModel.isUnavailable}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  isUnavailable: e.target.checked,
                };
                setEditProductModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldIsHiddenFromPublic">Is Hidden</label>{" "}
            <input
              id="fieldIsHiddenFromPublic"
              type="checkbox"
              checked={editProductModel.isHiddenFromPublic}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  isHiddenFromPublic: e.target.checked,
                };
                setEditProductModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldIsRecurring">Is Recurring</label>{" "}
            <input
              id="fieldIsRecurring"
              type="checkbox"
              checked={editProductModel.isRecurring}
              onChange={(e) => {
                const model: ProductModel = {
                  ...editProductModel,
                  isRecurring: e.target.checked,
                };
                if (model.isRecurring) {
                  if (model.installmentType === "OneTime") {
                    model.installmentType = "Monthly";
                  }
                  model.installments = 0;
                  model.lifeCycle = 0;
                } else {
                  if (model.installmentType === "Monthly") {
                    model.installments = 60;
                  }
                  if (model.installmentType === "Yearly") {
                    model.installments = 5;
                  }
                  model.lifeCycle = 60;
                }
                setEditProductModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldInstallmentType">Installment Type</label>
            <select
              className="form-control form-select"
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
              {!editProductModel.isRecurring && (
                <option value="OneTime">One Time</option>
              )}

              <option value="Monthly">Monthly</option>
              <option value="Yearly">Yearly</option>
            </select>
          </div>
          {editProductModel.installmentType === "OneTime" ||
          editProductModel.isRecurring ? null : (
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
          {!editProductModel.isRecurring && (
            <div className="form-group">
              <label htmlFor="lifeCycle">Life Cycle in Months</label>
              <input
                className="form-control"
                id="lifeCycle"
                required
                defaultValue={editProductModel.lifeCycle}
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
          )}
        </>
      ),

      //TODO: This should check that the installment and lifecycle are more than 0 when it isn't a recurring product
      canConfirm:
        !notification.pending &&
        notEmptyOrFalsey(editProductModel.name) &&
        notEmptyOrFalsey(editProductModel.category) &&
        notEmptyOrFalsey(editProductModel.units) &&
        notEmptyOrFalsey(editProductModel.description) &&
        parseFloat(editProductModel.unitPrice) > 0,
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
    columnHelper.accessor("isRecurring", {
      header: "Recurring",
      id: "isRecurring",
      cell: (value) => (value.row.original.isRecurring ? "Yes" : "No"),
      filterFn: (row, id, filterValue) => {
        return (row.original.isRecurring ? "YES" : "NO").startsWith(
          filterValue?.toUpperCase(),
        );
      },
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
          <ShowFor condition={row.original.isUnavailable}>
            <span className="badge bg-danger">Unavailable</span>{" "}
          </ShowFor>
          <ShowFor condition={row.original.isHiddenFromPublic}>
            <span className="badge bg-danger">Hidden</span>{" "}
          </ShowFor>
          <ShowFor
            condition={!row.original.isUnavailable}
            roles={["ClusterAdmin", "GroupAdmin"]}
          >
            <Link
              className="btn btn-primary"
              to={`/${cluster}/order/create/${row.original.id}`}
            >
              Order
            </Link>{" "}
          </ShowFor>{" "}
          <ShowFor roles={["ClusterAdmin", "FinancialAdmin"]}>
            <HipButton onClick={() => handleEdit(row.original.id)}>
              Edit
            </HipButton>{" "}
            <HipButton
              onClick={() => handleDelete(row.original.id)}
              color="danger"
            >
              Delete
            </HipButton>
          </ShowFor>
        </div>
      ),
    }),
  ];

  if (products === undefined) {
    // RH TODO: suspense/error boundaries
    return (
      <HipMainWrapper>
        <HipTitle title="Products" subtitle="Products" />
        <HipBody>
          <HipLoadingTable />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
      <HipMainWrapper>
        <HipTitle
          title="Order Products"
          subtitle="Products"
          buttons={
            <ShowFor roles={["ClusterAdmin", "FinancialAdmin"]}>
              <>
                <HipErrorBoundary>
                  <HipButton className="btn btn-primary" onClick={handleCreate}>
                    {" "}
                    Add Product{" "}
                  </HipButton>{" "}
                  <ShowFor roles={["ClusterAdmin"]}>
                    <Link
                      className="btn btn-primary"
                      to={`/${cluster}/order/create`}
                    >
                      {" "}
                      Adhoc Order{" "}
                    </Link>{" "}
                  </ShowFor>
                </HipErrorBoundary>
              </>
            </ShowFor>
          }
        />
        <HipBody>
          <HipErrorBoundary
            fallback={
              <HipClientError
                type="alert"
                thereWasAnErrorLoadingThe="Products Table"
              />
            }
          >
            <HipTable columns={columns} data={products} />
          </HipErrorBoundary>
        </HipBody>
      </HipMainWrapper>
    );
  }
};
