
<?php
require_once __DIR__ . '/vendor/autoload.php';

use Phpml\Dataset\CsvDataset;
use Phpml\Classification\SVC;
use Phpml\SupportVectorMachine\Kernel;
use Phpml\ModelManager;

// File paths for the model and dataset
define('MODEL_FILE', 'model.svm');
define('DATASET_FILE', 'dataset.csv');

// Check the request method and path
$requestMethod = $_SERVER['REQUEST_METHOD'];
$requestUri = $_SERVER['REQUEST_URI'];

// Routing logic
if ($requestMethod === 'POST' && strpos($requestUri, '/train') !== false) {
    trainModel();
} elseif ($requestMethod === 'POST' && strpos($requestUri, '/tain_new_data') !== false) {
    trainNewData(); // Call the new method for adding new data
}elseif ($requestMethod === 'POST' && strpos($requestUri, '/predict') !== false) {
    predictData();
} else {
    echo json_encode(['error' => 'Invalid endpoint or method.']);
    http_response_code(404);
}
// Function to train the model with new data and add it to the CSV file
function trainNewData()
{
    try {
        // Parse JSON input
        $input = json_decode(file_get_contents('php://input'), true);

        // Validate that the input is an array
        if (!is_array($input)) {
            throw new Exception('Invalid input format. Expecting an array of JSON objects with "features" and "label" fields.');
        }

        // Open the CSV file for appending
        $file = fopen(DATASET_FILE, 'a');
        if ($file === false) {
            throw new Exception('Unable to open dataset file for writing.');
        }

        // Process each item in the input array
        foreach ($input as $row) {
            if (!isset($row['features']) || !isset($row['label'])) {
                throw new Exception('Invalid input format. Each item must have "features" and "label" fields.');
            }

            $features = $row['features'];
            $label = $row['label'];

            // Validate the format of features and label
            if (!is_array($features) || !is_numeric($label)) {
                throw new Exception('Invalid data format. Features should be an array and label should be numeric.');
            }

            // Merge features and label into a single row and write it to the CSV file
            fputcsv($file, array_merge($features, [$label]));
        }

        fclose($file);

        // Reload the dataset with the newly appended data
        $dataset = new CsvDataset(DATASET_FILE, 4, true); // Assuming the label is at index 4
        $samples = $dataset->getSamples();
        $labels = $dataset->getTargets();

        // Retrain the model with the updated dataset
        $classifier = new SVC(Kernel::LINEAR, $cost = 1000);
        $classifier->train($samples, $labels);

        // Save the retrained model
        $modelManager = new ModelManager();
        $modelManager->saveToFile($classifier, MODEL_FILE);

        // Return the number of rows (samples) in the updated dataset
        $numRows = count($samples);
        echo json_encode([
            'status' => 'New data appended, model retrained successfully.',
            'num_rows' => $numRows
        ]);

    } catch (Exception $e) {
        echo json_encode(['error' => 'Failed to append data or retrain model: ' . $e->getMessage()]);
    }
}




// Function to train the model
function trainModel()
{
    try {
        $dataset = new CsvDataset(DATASET_FILE, 4, true);
        $samples = $dataset->getSamples();
        $labels = $dataset->getTargets();

        $classifier = new SVC(Kernel::LINEAR, $cost = 1000);
        $classifier->train($samples, $labels);

        $modelManager = new ModelManager();
        $modelManager->saveToFile($classifier, MODEL_FILE);
         $numRows = count($samples);

        echo json_encode(['status' => 'Model trained and saved successfully.', 'num_rows' => $numRows]);
    } catch (Exception $e) {
        echo json_encode(['error' => 'Failed to train model: ' . $e->getMessage()]);
    }
}

// Function to predict new data
function predictData()
{
    try {
        $input = json_decode(file_get_contents('php://input'), true);

        if (!isset($input['features']) || !is_array($input['features'])) {
            throw new Exception('Invalid input format. Expecting JSON with a "features" array of arrays.');
        }

        $featuresList = $input['features']; // Expecting an array of arrays (multiple rows)
        $modelManager = new ModelManager();
        $classifier = $modelManager->restoreFromFile(MODEL_FILE);

        $predictions = [];
        foreach ($featuresList as $features) {
            $predictions[] = $classifier->predict($features);
        }



        echo json_encode(['predictions' => $predictions]);
    } catch (Exception $e) {
        echo json_encode(['error' => 'Failed to predict: ' . $e->getMessage()]);
    }
}
?>

