import React, { useEffect, useState } from "react";

import { HistoryTable } from "./HistoryTable";

const OrderHistories: React.FC = () => {
  return <HistoryTable numberOfRows={1000} />;
};

export default OrderHistories;
