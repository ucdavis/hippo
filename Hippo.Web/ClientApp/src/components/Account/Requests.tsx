import { useEffect, useState, useMemo, useCallback } from "react";
import { RequestModel, IRouteParams } from "../../types";
import { RejectRequest } from "../../Shared/RejectRequest";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { useParams } from "react-router-dom";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";
import { SplitCamelCase, getGroupModelString } from "../../util/StringHelpers";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";

export const Requests = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each request

  const [requests, setRequests] = useState<RequestModel[]>();
  const [requestApproving, setRequestApproving] = useState<number>();
  const [notification, setNotification] = usePromiseNotification();

  const { cluster } = useParams<IRouteParams>();

  useEffect(() => {
    const fetchRequests = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/request/pending`
      );

      if (response.ok) {
        setRequests(await response.json());
      }
    };

    fetchRequests();
  }, [cluster]);

  const handleApprove = useCallback(
    async (request: RequestModel) => {
      setRequestApproving(request.id);

      const req = authenticatedFetch(
        `/api/${cluster}/request/approve/${request.id}`,
        {
          method: "POST",
        }
      );

      setNotification(
        req,
        "Approving",
        "Request Approved. Please allow 2 to 3 hours for changes to take place."
      );

      const response = await req;
      if (response.ok) {
        setRequestApproving(undefined);

        // remove the request from the list
        setRequests(requests?.filter((a) => a.id !== request.id));
      }
    },
    [requests, cluster, setNotification]
  );

  const handleReject = useCallback(
    async (request: RequestModel) => {
      // remove the request from the list
      setRequests(requests?.filter((a) => a.id !== request.id));
    },
    [requests]
  );

  const columns: Column<RequestModel>[] = useMemo(
    () => [
      {
        Header: "Request",
        accessor: (request) => SplitCamelCase(request.action),
        sortable: true,
      },
      {
        Header: "Name",
        accessor: (request) => request.requesterName,
        sortable: true,
      },
      {
        Header: "Email",
        accessor: (request) => request.requesterEmail,
        sortable: true,
      },
      {
        Header: "Group",
        accessor: (row) => getGroupModelString(row.groupModel),
        Cell: (props) => (
          <GroupNameWithTooltip
            group={props.row.original.groupModel}
            showDisplayName={false}
          />
        ),
        sortable: true,
      },
      {
        Header: "Supervising PI",
        accessor: (request) => request.supervisingPI,
        sortable: true,
      },
      {
        Header: "Action",
        sortable: false,
        Cell: (props) => (
          <>
            <button
              id="approveButton"
              disabled={notification.pending}
              onClick={() => handleApprove(props.row.original)}
              className="btn btn-primary"
            >
              {requestApproving === props.row.original.id
                ? "Approving..."
                : "Approve"}
            </button>
            {" | "}
            {requestApproving !== props.row.original.id && (
              <RejectRequest
                request={props.row.original}
                removeAccount={() => handleReject(props.row.original)}
                updateUrl={`/api/${cluster}/request/reject/`}
                disabled={notification.pending}
              ></RejectRequest>
            )}
          </>
        ),
      },
    ],
    [
      requestApproving,
      cluster,
      notification.pending,
      handleApprove,
      handleReject,
    ]
  );

  const accountsData = useMemo(() => requests ?? [], [requests]);

  if (requests === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">
          <p>There are {requests.length} request(s) awaiting approval</p>
          <ReactTable
            columns={columns}
            data={accountsData}
            initialState={{
              sortBy: [{ id: "Request" }, { id: "Name" }],
            }}
          />
        </div>
      </div>
    );
  }
};
