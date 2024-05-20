import React, { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import {
  HistoryModel,
  OrderMetadataModel,
  OrderModel,
  PaymentModel,
} from "../../types";
import { authenticatedFetch } from "../../util/api";
import { Column } from "react-table";
import { ReactTable } from "../../Shared/ReactTable";
import ChartStringValidation from "./ChartStringValidation";

export const Details = () => {
  const { cluster, orderId } = useParams();
  const [order, setOrder] = useState<OrderModel | null>(null);
  const [balanceRemaining, setBalanceRemaining] = useState<number>(0);
  const [balancePending, setBalancePending] = useState<number>(0);

  useEffect(() => {
    const fetchOrder = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/get/${orderId}`,
      );

      if (response.ok) {
        const data = await response.json();
        setOrder(data);
        const balanceRemaining = parseFloat(data.balanceRemaining);
        setBalanceRemaining(balanceRemaining);
        const balancePending = data?.payments
          .filter((payment) => payment.status !== "Completed")
          .reduce((acc, payment) => acc + parseFloat(payment.amount), 0);
        setBalancePending(balancePending);
      } else {
        alert("Error fetching order");
      }
    };

    fetchOrder();
  }, [cluster, orderId]);

  const historyColumns = useMemo<Column<HistoryModel>[]>(
    () => [
      {
        Header: "Date",
        accessor: "actedDate",
        Cell: ({ value }) => <span>{new Date(value).toLocaleString()}</span>,
      },
      {
        Header: "Actor",
        accessor: "actedBy",
        Cell: ({ value }) => (
          <span>
            {value.firstName} {value.lastName} ({value.email})
          </span>
        ),
      },
      {
        Header: "Status",
        accessor: "status",
      },
      {
        Header: "Details",
        accessor: "details",
      },
    ],
    [],
  );

  const metadataColumns = useMemo<Column<OrderMetadataModel>[]>(
    () => [
      {
        Header: "Name",
        accessor: "name",
      },
      {
        Header: "Value",
        accessor: "value",
      },
    ],
    [],
  );

  const paymentColumns = useMemo<Column<PaymentModel>[]>(
    () => [
      {
        Header: "Amount",
        accessor: "amount",
        Cell: ({ value }) => (
          <span>
            <i className="fas fa-dollar-sign" /> {value.toFixed(2)}
          </span>
        ),
      },
      {
        Header: "Status",
        accessor: "status",
      },
      {
        Header: "Created On",
        accessor: "createdOn",
        Cell: ({ value }) => <span>{new Date(value).toLocaleString()}</span>,
      },
      {
        Header: "Created By",
        accessor: "createdBy",
        Cell: ({ value }) =>
          (value && (
            <span>
              {value?.firstName} {value?.lastName} ({value?.email})
            </span>
          )) || <span>System</span>,
      },
    ],
    [],
  );

  if (!order) {
    return <div>Loading...</div>;
  }

  return (
    <div>
      <div className="row justify-content-center">
        <div className="col-md-8">
          <h1>Order Details: Id {order.id}</h1>
          <div className="form-group">
            <label htmlFor="fieldStatus">Status</label>
            <input
              className="form-control"
              id="fieldStatus"
              value={order.status}
              readOnly
            />
          </div>
          <hr />
          <div className="form-group">
            <label htmlFor="fieldName">Name</label>
            <input
              className="form-control"
              id="fieldName"
              required
              value={order.name}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldDescription">Description</label>
            <textarea
              className="form-control"
              id="fieldDescription"
              value={order.description || ""}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldCategory">Category</label>
            <input
              className="form-control"
              id="fieldCategory"
              value={order.category}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldExternalReference">External Reference</label>
            <input
              className="form-control"
              id="fieldExternalReference"
              value={order.externalReference}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldNotes">Notes</label>
            <textarea
              className="form-control"
              id="fieldNotes"
              value={order.notes || ""}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldUnits">Units</label>
            <input
              className="form-control"
              id="fieldUnits"
              value={order.units}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldUnitPrice">Unit Price</label>
            <div className="input-group">
              <div className="input-group-prepend">
                <span className="input-group-text" style={{ height: "38px" }}>
                  <i className="fas fa-dollar-sign" />
                </span>
              </div>
              <input
                className="form-control"
                id="fieldUnitPrice"
                value={order.unitPrice}
                readOnly
              />
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="fieldQuantity">Quantity</label>
            <input
              className="form-control"
              id="fieldQuantity"
              value={order.quantity}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldInstallments">Installments</label>
            <input
              className="form-control"
              id="fieldInstallments"
              value={order.installments}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldAdjustment">Adjustment</label>
            <div className="input-group">
              <div className="input-group-prepend">
                <span className="input-group-text" style={{ height: "38px" }}>
                  <i className="fas fa-dollar-sign" />
                </span>
              </div>
              <input
                className="form-control"
                id="fieldAdjustment"
                value={order.adjustment}
                readOnly
              />
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="fieldAdjustmentReason">Adjustment Reason</label>
            <textarea
              className="form-control"
              id="fieldAdjustmentReason"
              value={order.adjustmentReason || ""}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldAdminNotes">Admin Notes</label>
            <textarea
              className="form-control"
              id="fieldAdminNotes"
              value={order.adminNotes || ""}
              readOnly
            />
          </div>
          <hr />
          <div className="form-group">
            <label htmlFor="fieldSubTotal">SubTotal</label>
            <div className="input-group">
              <div className="input-group-prepend">
                <span className="input-group-text" style={{ height: "38px" }}>
                  <i className="fas fa-dollar-sign" />
                </span>
              </div>
              <div className="form-group">
                <input
                  className="form-control"
                  id="fieldSubTotal"
                  value={order.subTotal}
                  readOnly
                />
              </div>
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="fieldTotal">Total</label>
            <div className="input-group">
              <div className="input-group-prepend">
                <span className="input-group-text" style={{ height: "38px" }}>
                  <i className="fas fa-dollar-sign" />
                </span>
              </div>
              <input
                className="form-control"
                id="fieldTotal"
                value={order.total}
                readOnly
              />
            </div>
          </div>

          <h2>Chart Strings</h2>
          <table className="table table-bordered table-striped">
            <thead>
              <tr>
                <th>Chart String</th>
                <th>Percent</th>
                <th>Chart String Validation</th>
              </tr>
            </thead>
            <tbody>
              {order.billings.map((billing) => (
                <tr key={billing.id}>
                  <td>{billing.chartString}</td>
                  <td>{billing.percentage}</td>
                  <td>
                    <ChartStringValidation
                      chartString={billing.chartString}
                      key={billing.chartString}
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <h2>Metadata</h2>
          <ReactTable columns={metadataColumns} data={order.metaData} />
          <h2>History</h2>
          <ReactTable
            columns={historyColumns}
            data={order.history}
            initialState={{
              sortBy: [{ id: "Date" }],
            }}
          />
          <h2>Payments</h2>
          <div className="form-group">
            <label htmlFor="fieldBalanceRemaining">Balance Remaining</label>
            <div className="input-group">
              <div className="input-group-prepend">
                <span className="input-group-text" style={{ height: "38px" }}>
                  <i className="fas fa-dollar-sign" />
                </span>
              </div>
              <input
                className="form-control"
                id="fieldBalanceRemaining"
                value={order.balanceRemaining}
                readOnly
              />
            </div>
          </div>
          {balancePending !== 0 && (
            <div className="form-group">
              <label htmlFor="fieldBalancePending">Balance Pending</label>
              <div className="input-group">
                <div className="input-group-prepend">
                  <span className="input-group-text" style={{ height: "38px" }}>
                    <i className="fas fa-dollar-sign" />
                  </span>
                </div>
                <input
                  className="form-control"
                  id="fieldBalancePending"
                  value={(balanceRemaining - balancePending).toFixed(2)}
                  readOnly
                />
              </div>
            </div>
          )}
          <br />
          {order.payments.length !== 0 && (
            <ReactTable columns={paymentColumns} data={order.payments} />
          )}
        </div>
      </div>
    </div>
  );
};
