This folder holds the SSH key pair used by the **KubernetesDebugger** to authenticate with
**vs-debug** ephemeral containers.  These files should **NEVER CHANGE** because doing so
will prevent older versions of the debbugger from connecting.  If these really need to change
for some reason, you'll need to update **KubernetesDebugger** with the new private key.

NOTE: I tried doing password-less SSH root authentication and that worked for Docker containers
      but does not work for Kubernetes.CRI-O.  Note that SSH security is redundant for debugging
      scenarios because the user will already be authenticated with the Kubernetes cluster and
      the API server will enforce RBAC permissions when the SSH tunnel to the ephemeral container
      is established.  So using fixed keys is not really a security problem.

id_rsa:

The private root SSH key which will be embedded in **KubernetesDebugger**.

id_rsa.pub:

The private root SSH key which will be appended to `/root/.ssh/authorized_keys`
when the container image is built.
