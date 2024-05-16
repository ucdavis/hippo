import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { OrderModel } from "../../types";
import { authenticatedFetch } from "../../util/api";

export const Details = () => {
  const { cluster, orderId } = useParams();
  const [order, setOrder] = useState<OrderModel | null>(null);

  useEffect(() => {
    const fetchOrder = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/get/${orderId}`,
      );

      if (response.ok) {
        const data = await response.json();
        setOrder(data);
      } else {
        alert("Error fetching order");
      }
    };

    fetchOrder();
  }, [cluster, orderId]);

  if (!order) {
    return <div>Loading...</div>;
  }

  console.log(order);

  return (
    <div>
      <h1>Order Details</h1>
      <p>Order ID: {order.id}</p>

      <p>Order Name: {order.name}</p>
      <p>History 1 : {order.history[0].details}</p>
      <p>History 2 : {order.history[0].actedBy.email}</p>
      {/* Add more details as needed */}
    </div>
  );
};
