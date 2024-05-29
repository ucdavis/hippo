import React, { useEffect, useState } from "react";
import { OrderModel } from "../../types";
import { useParams } from "react-router-dom";
import { usePermissions } from "../../Shared/usePermissions1";
import { usePromiseNotification } from "../../util/Notifications";
import OrderForm from "./OrderForm";
import { authenticatedFetch, parseBadRequest } from "../../util/api";

const defaultOrder: OrderModel = {
  id: 0,
  category: "",
  name: "",
  productName: "",
  description: "",
  notes: "",
  units: "",
  unitPrice: "",
  installments: 5,
  installmentType: "Yearly",
  quantity: 0,
  adjustment: 0,
  adjustmentReason: "",
  status: "N/A",
  createdOn: "",
  externalReference: "",
  adminNotes: "",
  subTotal: "",
  total: "",
  balanceRemaining: "",
  metaData: [],
  billings: [],
  payments: [],
  history: [],
};
const CreateOrder: React.FC = () => {
  const { cluster, productId } = useParams();
  const { isClusterAdminForCluster } = usePermissions();
  const [order, setOrder] = useState<OrderModel>(null);
  const [isClusterAdmin, setIsClusterAdmin] = useState(null);
  const [notification, setNotification] = usePromiseNotification();

  useEffect(() => {
    setIsClusterAdmin(isClusterAdminForCluster());
  }, [isClusterAdmin, isClusterAdminForCluster]);

  useEffect(() => {
    if (productId) {
      const fetchProductOrder = async () => {
        const response = await authenticatedFetch(
          `/api/${cluster}/order/GetProduct/${productId}`,
        );

        if (response.ok) {
          const data = await response.json();
          setOrder(data);
          // const balanceRemaining = parseFloat(data.balanceRemaining);
          // setBalanceRemaining(balanceRemaining);
          // const balancePending = data?.payments
          //   .filter((payment) => payment.status !== "Completed")
          //   .reduce((acc, payment) => acc + parseFloat(payment.amount), 0);
          // setBalancePending(balancePending);
        } else {
          alert("Error fetching product for order");
        }
      };

      fetchProductOrder();
    } else {
      if (isClusterAdmin === false) {
        window.location.href = `/${cluster}/product/index`;
      } else {
        setOrder(defaultOrder);
      }
    }
  }, [cluster, isClusterAdmin, productId]);

  // async function so the form can manage the loading state
  const submitOrder = async (updatedOrder: OrderModel) => {
    const editedOrder: OrderModel = {
      // uneditable fields
      id: updatedOrder.id,
      status: updatedOrder.status,
      createdOn: updatedOrder.createdOn,
      total: updatedOrder.total,
      subTotal: updatedOrder.subTotal,
      balanceRemaining: updatedOrder.balanceRemaining,
      billings: updatedOrder.billings,
      payments: updatedOrder.payments,
      history: updatedOrder.history,

      // editable fields
      name: updatedOrder.name,
      productName: updatedOrder.productName,
      description: updatedOrder.description,
      category: updatedOrder.category,
      externalReference: updatedOrder.externalReference,
      notes: updatedOrder.notes,
      units: updatedOrder.units,
      unitPrice: updatedOrder.unitPrice,
      quantity: updatedOrder.quantity,
      installments: updatedOrder.installments,
      installmentType: updatedOrder.installmentType,
      adjustment: updatedOrder.adjustment,
      adjustmentReason: updatedOrder.adjustmentReason,
      adminNotes: updatedOrder.adminNotes,
      metaData: updatedOrder.metaData,
    };

    const req = authenticatedFetch(`/api/${cluster}/order/Save`, {
      method: "POST",
      body: JSON.stringify(editedOrder),
    });

    setNotification(req, "Saving", "Order Saved", async (r) => {
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

      window.location.href = `/${cluster}/order/details/${data.id}`;
    }

    setOrder(editedOrder); // should be newOrder once it's pulling from the API
  };

  if (isClusterAdmin === null) {
    return <div>Loading...</div>;
  }

  if (!order) {
    return <div>Loading... {productId}</div>;
  }

  return (
    <div>
      {order && (
        <div>
          <h2>Create Order</h2>

          <OrderForm
            orderProp={order}
            readOnly={false}
            onSubmit={submitOrder}
          />
          {notification.pending && <div>Saving...</div>}
        </div>
      )}
    </div>
  );
};

export default CreateOrder;
