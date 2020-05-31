﻿using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningForKids
{
    public class GenerateModel
    {
        public ITransformer TrainModel(MLContext mlContext,string dataName)
        {
            
            var data = mlContext.Data.LoadFromTextFile<ImageData>(dataName, separatorChar: ',');

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("LabelKey", "Label")
                .Append(mlContext.Transforms.LoadImages("input", "images", nameof(ImageData.ImagePath)))
                .Append(mlContext.Transforms.ResizeImages("input", InceptionSettings.ImageWidht,
                InceptionSettings.ImageHeight, "input"))
                .Append(mlContext.Transforms.ExtractPixels("input", interleavePixelColors: InceptionSettings.ChannelsList,
                offsetImage: InceptionSettings.Mean))
                .Append(mlContext.Model.LoadTensorFlowModel("./model/tensorflow_inception_graph.pb")
                .ScoreTensorFlowModel(new[] { "softmax2_pre_activation" }, new[] { "input" }, addBatchDimensionInput: true))
                .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy("LabelKey", "softmax2_pre_activation"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabelValue", "PredictedLabel"));

            var model = pipeline.Fit(data);

            // TEST İşlemleri Kendimiz Canlı veri ile test ediyoruz diye kullanmadık.

            //IDataView testData = mlContext.Data.LoadFromTextFile<ImageData>(path: _testTagsTsv, hasHeader: false);
            //IDataView predictions = model.Transform(testData);

            //MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictions,labelColumnName: "LabelKey",predictedLabelColumnName: "PredictedLabel");

            //Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            //Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            return model;

        }

        
    }
}
