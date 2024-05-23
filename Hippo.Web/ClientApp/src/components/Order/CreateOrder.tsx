import React, { useEffect, useState } from "react";
import { OrderModel } from "../../types";
import { useParams } from "react-router-dom";

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
  const [order, setOrder] = useState<OrderModel>({ ...defaultOrder });

  useEffect(() => {
    if (productId) {
      setOrder((order) => ({ ...order, name: productId }));
    } else {
      setOrder((order) => ({ ...order, name: "No Product ID" }));
    }
  }, [productId]);

  if (!order) {
    return <div>Loading... {productId}</div>;
  }

  return (
    <div>
      {order && (
        <div>
          <h2>Order Details</h2>

          <p>Product ID: {order.name}</p>
          <p>Status: {order.status}</p>
        </div>
      )}
    </div>
  );
};

export default CreateOrder;
