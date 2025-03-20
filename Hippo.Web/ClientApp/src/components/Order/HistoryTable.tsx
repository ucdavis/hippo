import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { HistoryModel } from "../../types";
import { HipTable } from "../../Shared/Table/HipTable";
import { convertToPacificTime } from "../../util/DateHelper";
import { createColumnHelper } from "@tanstack/react-table";
import { authenticatedFetch } from "../../util/api";
import { useParams } from "react-router-dom";
import HipButton from "../../Shared/HipComponents/HipButton";

interface HistoryTableProps {
  numberOfRows: number;
  showLinkToAll: boolean;
  historyCount: number;
}

export const HistoryTable: React.FC<HistoryTableProps> = ({
  numberOfRows,
  showLinkToAll,
  historyCount,
}) => {
  const [histories, setHistories] = useState<HistoryModel[]>([]);
  const { cluster, orderId } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchHistories = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/GetHistories/${orderId}?max=${numberOfRows}`,
      );

      if (response.ok) {
        const data = await response.json();

        setHistories(data);
      } else {
        alert("Error fetching histories");
      }
    };

    fetchHistories();
  }, [cluster, numberOfRows, orderId, historyCount]);

  const historyColumnHelper = createColumnHelper<HistoryModel>();

  const historyColumns = [
    historyColumnHelper.accessor("actedDate", {
      header: "Date",
      id: "actedDate",
      cell: (value) => (
        <span>{convertToPacificTime(value.row.original.actedDate)}</span>
      ),
      sortingFn: (rowA, rowB, columnId) => {
        const dateA = new Date(rowA.getValue(columnId)).getTime();
        const dateB = new Date(rowB.getValue(columnId)).getTime();
        return dateA - dateB; // Sort by raw timestamp
      },
    }),
    historyColumnHelper.accessor("actedBy", {
      header: "Actor",
      id: "actedBy",
      cell: (value) => (
        <>
          {value.row.original.actedBy ? (
            <>
              {value.row.original.actedBy.name} (
              {value.row.original.actedBy.email})
            </>
          ) : (
            <>System</>
          )}
        </>
      ),
    }),
    historyColumnHelper.accessor("status", { header: "Status", id: "status" }),
    historyColumnHelper.accessor("details", {
      header: "Details",
      id: "details",
    }),
  ];

  if (!histories) {
    return <div>Loading Histories...</div>;
  }

  return (
    <>
      <h2>History</h2>
      <small>Last {numberOfRows} or fewer Actions</small>{" "}
      {showLinkToAll ? (
        <HipButton
          onClick={() => navigate(`/${cluster}/order/history/${orderId}`)}
          className="float-right"
          color="link"
        >
          View All
        </HipButton>
      ) : (
        <HipButton
          onClick={() => navigate(`/${cluster}/order/details/${orderId}`)}
          className="float-right"
          color="link"
        >
          Back to Order Details
        </HipButton>
      )}
      <HipTable
        columns={historyColumns}
        data={histories}
        disablePagination={numberOfRows <= 5} // since we are only showing past 5 actions
        disableFilter={numberOfRows <= 5}
        initialState={{
          sorting: [
            {
              id: "actedDate",
              desc: true,
            },
          ],
        }}
      />
    </>
  );
};
