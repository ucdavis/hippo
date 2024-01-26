import { useContext, useEffect, useState } from "react";
import "react-bootstrap-typeahead/css/Typeahead.css";
import { useNavigate, useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import { GroupModel, AccountCreateModel, RequestModel } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { GroupLookup } from "../Group/GroupLookup";
import SshKeyInput from "../../Shared/SshKeyInput";

export const RequestForm = () => {
  const [context, setContext] = useContext(AppContext);
  const [notification, setNotification] = usePromiseNotification();

  const [groups, setGroups] = useState<GroupModel[]>([]);
  const [request, setRequest] = useState<AccountCreateModel>({
    groupId: 0,
    sshKey: "",
    supervisingPI: "",
  });

  const navigate = useNavigate();
  const { cluster: clusterName } = useParams();
  const cluster = context.clusters.find((c) => c.name === clusterName);

  // load up possible groups
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(
        `/api/${clusterName}/group/groups`,
      );

      const groupsResult = await response.json();

      if (response.ok) {
        setGroups(groupsResult);
      }
    };

    fetchGroups();
  }, [clusterName]);

  const handleSubmit = async () => {
    const req = authenticatedFetch(`/api/${clusterName}/account/create`, {
      method: "POST",
      body: JSON.stringify(request),
    });

    setNotification(
      req,
      "Saving",
      "Request Created. Please wait for your sponsor to approve your request.",
      async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      },
    );

    const response = await req;

    if (response.ok) {
      const request = (await response.json()) as RequestModel;

      setContext((ctx) => ({
        ...ctx,
        openRequests: [...ctx.openRequests, { ...request }],
      }));
      navigate(`/${clusterName}/pendingapproval`);
    }
  };

  useEffect(() => {
    if (
      context.openRequests.find(
        (r) => r.cluster === clusterName && r.action === "CreateAccount",
      )
    ) {
      // there's already a request for this cluster, redirect to pending page
      navigate(`/${clusterName}/pendingapproval`);
    }
  }, [clusterName, context.openRequests, navigate]);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <h3>
          Welcome,{" "}
          <span className="status-color">{context.user.detail.firstName}</span>
        </h3>
        <p>
          You don't seem to have an account on this cluster yet. If you'd like
          access, please answer the&nbsp;questions&nbsp;below
        </p>
        <hr />
        <div className="form-group">
          <label>Who is sponsoring your account?</label>
          <GroupLookup
            setSelection={(group) =>
              setRequest((r) => ({ ...r, groupId: group.id }))
            }
            options={groups}
          />
          <p className="form-helper">
            Your group is probably named after your PI or your Department. You
            can filter this list by typing in it. If you don't see your sponsor,
            please contact your PI and ask them to request an account,
            specifying "New Sponsor Onboarding" as the group.
          </p>
        </div>
        <div className="form-group">
          <label className="form-label">Who is your supervising PI?</label>
          <input
            className="form-control"
            id="supervisingPI"
            placeholder="Supervising PI"
            value={request.supervisingPI}
            onChange={(e) =>
              setRequest((r) => ({ ...r, supervisingPI: e.target.value }))
            }
          ></input>
          <p className="form-helper">
            Some clusters may require additional clarification on who your
            supervising PI will be. If you are unsure, please ask your sponsor.
          </p>
        </div>
        {cluster.enableUserSshKey && (
          <div className="form-group">
            <label className="form-label">
              Please paste your public SSH key.
            </label>
            <SshKeyInput
              onChange={(value) => setRequest((r) => ({ ...r, sshKey: value }))}
            />
          </div>
        )}
        <br />
        <button
          disabled={notification.pending}
          onClick={handleSubmit}
          className="btn btn-primary"
        >
          Submit
        </button>
        {cluster.enableUserSshKey && (
          <div>
            <br />
            <br />
            <p className="form-helper">
              To generate a ssh key pair please see this link:{" "}
              <a href="https://hpc.ucdavis.edu/faq#ssh-key" target={"blank"}>
                https://hpc.ucdavis.edu/faq#ssh-key
              </a>
            </p>
            <p className="form-helper">
              You will find instructions here for Windows, OS X, and Linux
              environments. Windows users: an openssh or SSH2 formatted ssh
              public key is required. See screenshots.
            </p>
            <p className="form-helper">
              (If your public key resides in the default location for OS X and
              Linux, ~/.ssh/, right click in the Name column of the window that
              will open when you click "Choose File" above. Select "Show Hidden
              Files".)
            </p>
            <p className="form-helper">
              For more information on SSH keys, please see{" "}
              <a
                href="https://wiki.cse.ucdavis.edu/support:general:security:ssh"
                target={"blank"}
              >
                https://wiki.cse.ucdavis.edu/support:general:security:ssh
              </a>
            </p>
          </div>
        )}
      </div>
    </div>
  );
};
