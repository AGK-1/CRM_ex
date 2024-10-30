 async function fetchUsers() {
            const token = localStorage.getItem('tokenkol'); // Get the token from local storage

            if (!token) {
                alert('You need to log in first.');
                window.location.href = 'login.html'; // Redirect to login page if not logged in
                return;
            }

            try {
                const response = await fetch('https://localhost:7259/api/Crm/get-users', {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`, // Include the token in the Authorization header
                        'Content-Type': 'application/json'
                    }
                });

                if (!response.ok) {
                    throw new Error('Failed to fetch users');
                }

                const users = await response.json();
                displayUsers(users); // Call a function to display users in HTML
            } catch (error) {
                console.error('There was a problem fetching the users:', error);
                alert('Error fetching users. Please check your login status.');
            }
        }

        async function fetchCustomers() {
            const token = localStorage.getItem('tokenkol'); // Get the token from local storage

            if (!token) {
                alert('You need to log in first.');
                window.location.href = 'login.html'; // Redirect to login page if not logged in
                return;
            }

            try {
                const response = await fetch('https://localhost:7259/api/Crm', {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`, // Include the token in the Authorization header
                        'Content-Type': 'application/json'
                    }
                });

                if (!response.ok) {
                    throw new Error('Failed to fetch customers');
                }

                const customers = await response.json();
                displayCustomers(customers); // Call a function to display customers in HTML
            } catch (error) {
                console.error('There was a problem fetching the customers:', error);
                alert('Error fetching customers. Please check your login status.');
            }
        }

        function displayUsers(users) {
            const table = document.getElementById('userTable').getElementsByTagName('tbody')[0];
            table.innerHTML = ''; // Clear existing data

            users.forEach(user => {
                const row = document.createElement('tr');
                row.innerHTML = `<td>${user.name}</td><td>${user.email}</td><td>${user.role}</td>`;
                table.appendChild(row);
            });
        }

        function displayCustomers(customers) {
            const table = document.getElementById('customerTable').getElementsByTagName('tbody')[0];
            table.innerHTML = ''; // Clear existing data

            customers.forEach(customer => {
                const row = document.createElement('tr');
                row.innerHTML = `<td>${customer.companyName}</td><td>${customer.contactName}</td><td>${customer.phone}</td>`;
                table.appendChild(row);
            });
        }

        // Fetch users and customers on page load
        fetchUsers();
        fetchCustomers();