﻿using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Zenject;

namespace TicTacToe3D
{
    public class SaveGameWindowPresenter : MenuPresenter<SaveGameWindowView>, IInitializable, IDisposable
    {
        private readonly MenuManager _menuManager;
        private readonly GameManager _gameManager;
        private readonly SaveItemModel.Registry _saveItemsRegistry;
        private readonly SaveItemFacade.Factory _saveItemFactory;
        private readonly IFetchService<History> _fetchService;

        public SaveGameWindowPresenter(MenuManager menuManager,
            GameManager gameManager,
            SaveItemModel.Registry saveItemsRegistry,
            SaveItemFacade.Factory saveItemFactory,
            IFetchService<History> fetchService)
        {
            _menuManager = menuManager;
            _gameManager = gameManager;
            _saveItemsRegistry = saveItemsRegistry;
            _saveItemFactory = saveItemFactory;
            _fetchService = fetchService;

            menuManager.SetMenu(this);
        }

        public void Initialize()
        {
            View.SaveButton.onClick.AddListener(OnSaveButtonClicked);
            View.BackButton.onClick.AddListener(OnBackButtonClicked);
        }

        public void Dispose()
        {
            View.SaveButton.onClick.RemoveAllListeners();
            View.BackButton.onClick.RemoveAllListeners();
        }

        private void OnSaveButtonClicked()
        {
            if (View.SaveNameInputField.text == string.Empty)
            {
                return;
            }
            _gameManager.SaveGame(View.SaveNameInputField.text);
            UpdateSaves();
        }

        private void OnBackButtonClicked()
        {
            _menuManager.OpenMenu(Menus.PauseWindow);
        }

        public override void Open()
        {
            UpdateSaves();
            base.Open();
        }

        private void UpdateSaves()
        {
            View.SaveNameInputField.text = string.Empty;
            View.SaveInformationText.text = string.Empty;
            _saveItemsRegistry.Clear();
            var dir = new DirectoryInfo(Application.dataPath + "/Saves/");
            if (dir.Exists == false)
            {
                return;
            }
            var saves = dir.GetFiles("*.json");
            foreach (var save in saves)
            {
                var saveItem = _saveItemFactory.Create();
                saveItem.transform.SetParent(View.SavesToggleGroup.transform, false);
                saveItem.SaveItemToggle.group = View.SavesToggleGroup;
                saveItem.SaveItemToggleText.text = Path.GetFileNameWithoutExtension(save.Name);
                saveItem.SaveItemToggle.onValueChanged.AddListener(OnSaveItemChecked);
                saveItem.History = _fetchService.Load(saveItem.SaveItemToggleText.text);
            }
        }

        private void OnSaveItemChecked(bool isChecked)
        {
            if (isChecked == false)
            {
                return;
            }
            var selectedItem = _saveItemsRegistry.Items.First(item => item.IsActive);
            View.SaveNameInputField.text = selectedItem.Name;
            View.SaveInformationText.text = string.Format("Dimension: {0}\nStep Size: {1}\nBadged To Win: {2}\nPlayers:\n{3}",
                selectedItem.History.Info.Dimension, selectedItem.History.Info.StepSize, selectedItem.History.Info.BadgesToWin,
                selectedItem.History.Info.Players.Aggregate("", (current, player) => current + "\t" + player.Name + "\n"));
        }
    }
}